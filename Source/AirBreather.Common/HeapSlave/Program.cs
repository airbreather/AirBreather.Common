using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace HeapSlave
{
    internal static class Program
    {
        private const int TooFewArgsExitCode = 1;
        private const int CacheNotFoundExitCode = 2;
        private const int GeneralErrorExitCode = 3;

        private static int Main(string[] args)
        {
            long afterAttach = 0;
            long afterCreateRuntime = 0;
            long afterGetHeap = 0;
            long beforeEnumerateObjects = 0;
            long afterEnumerateObjects = 0;
            long afterEnumerateRoots = 0;
            Stopwatch sw = Stopwatch.StartNew();
            BitArray found = null;
            try
            {
                int pid;
                if (args.Length < 2)
                {
                    return TooFewArgsExitCode;
                }

                pid = Int32.Parse(args[0], NumberStyles.None, CultureInfo.InvariantCulture);
                Guid cache = Guid.ParseExact(args[1], "N");

                byte[] guidBuffer = new byte[16];
                using (DataTarget dataTarget = DataTarget.AttachToProcess(pid, UInt32.MaxValue))
                {
                    afterAttach = sw.ElapsedTicks;
                    foreach (ClrInfo clrVersion in dataTarget.ClrVersions)
                    {
                        ClrRuntime runtime = clrVersion.CreateRuntime();
                        afterCreateRuntime = sw.ElapsedTicks;
                        ClrHeap heap = runtime.GetHeap();
                        afterGetHeap = sw.ElapsedTicks;

                        ClrType cacheType = null, listType = null;
                        ClrInstanceField idField = null, storedField = null, itemsField = null, countField = null;

                        foreach (ClrType type in heap.EnumerateTypes())
                        {
                            if (type.Name == "AirBreather.Common.Caching.MemoryCachePlus")
                            {
                                cacheType = type;
                                foreach (ClrInstanceField field in cacheType.Fields)
                                {
                                    if (field.Name == "id")
                                    {
                                        idField = field;
                                    }
                                    else if (field.Name == "stored")
                                    {
                                        storedField = field;
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    if (idField != null && storedField != null)
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (type.Name == "System.Collections.Generic.List")
                            {
                                listType = type;
                                foreach (ClrInstanceField field in listType.Fields)
                                {
                                    if (field.Name == "_items")
                                    {
                                        itemsField = field;
                                    }
                                    else if (field.Name == "_size")
                                    {
                                        countField = field;
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    if (itemsField != null && countField != null)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }

                            if (listType != null && cacheType != null)
                            {
                                break;
                            }
                        }

                        beforeEnumerateObjects = sw.ElapsedTicks;

                        ulong cacheAddress = 0;
                        foreach (ulong obj in heap.EnumerateObjectAddresses().Where(obj => heap.GetObjectType(obj) == cacheType))
                        {
                            ulong idAddress = idField.GetAddress(obj);
                            heap.ReadMemory(idAddress, guidBuffer, 0, 16);

                            if (new Guid(guidBuffer) != cache)
                            {
                                continue;
                            }

                            cacheAddress = obj;
                            break;
                        }

                        afterEnumerateObjects = sw.ElapsedTicks;
                        if (cacheAddress == 0)
                        {
                            continue;
                        }

                        ulong stored = (ulong)storedField.GetValue(cacheAddress);
                        ulong items = (ulong)itemsField.GetValue(stored);
                        int cnt = (int)countField.GetValue(stored);
                        byte[] itemAddressesBytes = new byte[cnt * sizeof(ulong)];
                        heap.ReadMemory(items + 16, itemAddressesBytes, 0, itemAddressesBytes.Length);
                        ulong[] itemAddresses = new ulong[cnt];
                        Buffer.BlockCopy(itemAddressesBytes, 0, itemAddresses, 0, itemAddressesBytes.Length);

                        found = new BitArray(cnt, false);
                        HashSet<ulong> visited = new HashSet<ulong>();
                        IEnumerable<ulong> rootAddresses = heap.EnumerateRoots(false).Select(root => root.Object).Where(visited.Add);

                        Dictionary<ulong, List<int>> initialRefs = new Dictionary<ulong, List<int>>();
                        for (int i = 0; i < itemAddresses.Length; i++)
                        {
                            ulong itemAddress = itemAddresses[i];
                            List<int> lst;
                            if (!initialRefs.TryGetValue(itemAddress, out lst))
                            {
                                initialRefs[itemAddress] = lst = new List<int>();
                            }

                            lst.Add(i);
                        }

                        Dictionary<ulong, int> unfound = new Dictionary<ulong, int>(cnt);
                        foreach (var kvp in initialRefs)
                        {
                            ulong itemAddress = kvp.Key;
                            List<int> lst = kvp.Value;

                            if (lst.Count < 2)
                            {
                                unfound.Add(itemAddress, lst.Single());
                            }
                            else
                            {
                                foreach (int i in lst)
                                {
                                    found[i] = true;
                                }
                            }
                        }

                        Stack<ulong> refs = new Stack<ulong>();
                        bool doingRoots = false;
                        using (IEnumerator<ulong> rootAddressEnumerator = rootAddresses.GetEnumerator())
                        {
                            while (refs.Count > 0 || (doingRoots = rootAddressEnumerator.MoveNext()))
                            {
                                ulong r = doingRoots ? rootAddressEnumerator.Current : refs.Pop();
                                doingRoots = false;

                                if (r == items)
                                {
                                    continue;
                                }

                                int i;
                                if (unfound.TryGetValue(r, out i))
                                {
                                    unfound.Remove(r);
                                    found[i] = true;
                                }

                                ClrType rType = heap.GetObjectType(r);
                                if (rType == null || !rType.ContainsPointers)
                                {
                                    continue;
                                }

                                rType.EnumerateRefsOfObject(r, (addr, _) =>
                                {
                                    if (visited.Add(addr))
                                    {
                                        refs.Push(addr);
                                    }
                                });
                            }
                        }

                        afterEnumerateRoots = sw.ElapsedTicks;

                        // now look for references from within the items collection
                        for (int i = 0; i < itemAddresses.Length; i++)
                        {
                            ulong targetAddress = itemAddresses[i];
                            if (!unfound.ContainsKey(targetAddress))
                            {
                                continue;
                            }

                            visited.Clear();
                            for (int j = 0; j < itemAddresses.Length; j++)
                            {
                                if (j == i)
                                {
                                    continue;
                                }

                                ulong refAddress = itemAddresses[j];
                                refs.Push(refAddress);
                            }

                            while (refs.Count > 0)
                            {
                                ulong r = refs.Pop();

                                if (r == items)
                                {
                                    continue;
                                }

                                if (r == targetAddress)
                                {
                                    unfound.Remove(r);
                                    found[i] = true;
                                }

                                ClrType rType = heap.GetObjectType(r);
                                if (rType == null || !rType.ContainsPointers)
                                {
                                    continue;
                                }

                                rType.EnumerateRefsOfObject(r, (addr, _) =>
                                {
                                    if (visited.Contains(addr))
                                    {
                                        return;
                                    }

                                    refs.Push(addr);
                                    visited.Add(addr);
                                });
                            }

                            refs.Clear();
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled exception:");
                Console.WriteLine(ex);
                return GeneralErrorExitCode;
            }

            if (found == null)
            {
                return CacheNotFoundExitCode;
            }

            for (int i = 0; i < found.Length; i++)
            {
                Console.WriteLine(String.Format(CultureInfo.InvariantCulture, "{0}_{1}", i.ToString(CultureInfo.InvariantCulture), found[i] ? "1" : "0"));
            }

            sw.Stop();
            using (var f = File.CreateText(@"C:\Users\PC\runtime.txt"))
            {
                double freq = Stopwatch.Frequency;
                f.WriteLine("afterAttach: {0:N5} seconds", afterAttach / freq);
                f.WriteLine("afterCreateRuntime: {0:N5} seconds", afterCreateRuntime / freq);
                f.WriteLine("afterGetHeap: {0:N5} seconds", afterGetHeap / freq);
                f.WriteLine("beforeEnumerateObjects: {0:N5} seconds", beforeEnumerateObjects / freq);
                f.WriteLine("afterEnumerateObjects: {0:N5} seconds", afterEnumerateObjects / freq);
                f.WriteLine("afterEnumerateRoots: {0:N5} seconds", afterEnumerateRoots / freq);
                f.WriteLine("end: {0:N5} seconds", sw.ElapsedTicks / freq);
            }

            return 0;
        }
    }
}
