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
            long beforeEnumerateObjects = 0;
            long afterEnumerateObjects = 0;
            long afterEnumerateRoots = 0;
            long[] visits = null;
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
                        ClrHeap heap = runtime.GetHeap();
                        ClrType cacheType = heap.GetTypeByName("AirBreather.Common.Caching.MemoryCachePlus");
                        ClrType listType = heap.GetTypeByName("System.Collections.Generic.List");

                        ClrInstanceField idField = cacheType.GetFieldByName("id");
                        ClrInstanceField storedField = cacheType.GetFieldByName("stored");
                        ClrInstanceField itemsField = listType.GetFieldByName("_items");
                        ClrInstanceField countField = listType.GetFieldByName("_size");

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
                        byte[] itemAddresses = new byte[cnt * sizeof(ulong)];
                        heap.ReadMemory(items + 16, itemAddresses, 0, itemAddresses.Length);

                        ulong[] rootAddresses = heap.EnumerateRoots(false).Select(root => root.Object).ToArray();

                        afterEnumerateRoots = sw.ElapsedTicks;
                        visits = new long[cnt];

                        found = new BitArray(cnt, false);
                        for (int i = 0; i < cnt; i++)
                        {
                            HashSet<ulong> visited = new HashSet<ulong>();
                            ulong address = BitConverter.ToUInt64(itemAddresses, i * sizeof(ulong));
                            Stack<ulong> refs = new Stack<ulong>(rootAddresses.Length + cnt - 1);

                            foreach (ulong rootAddress in rootAddresses)
                            {
                                if (visited.Add(rootAddress))
                                {
                                    refs.Push(rootAddress);
                                }
                            }

                            for (int j = 0; j < cnt; j++)
                            {
                                if (j == i)
                                {
                                    continue;
                                }

                                ulong otherAddress = BitConverter.ToUInt64(itemAddresses, j * sizeof(ulong));
                                refs.Push(otherAddress);
                                visited.Add(otherAddress);
                            }

                            while (refs.Count > 0)
                            {
                                ulong r = refs.Pop();
                                if (r == address)
                                {
                                    found[i] = true;
                                    break;
                                }

                                if (r == items)
                                {
                                    continue;
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

                            visits[i] = sw.ElapsedTicks;
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
                f.WriteLine("beforeEnumerateObjects: {0:N5} seconds", beforeEnumerateObjects / freq);
                f.WriteLine("afterEnumerateObjects: {0:N5} seconds", afterEnumerateObjects / freq);
                f.WriteLine("afterEnumerateRoots: {0:N5} seconds", afterEnumerateRoots / freq);
                for (int i = 0; i < visits.Length; i++)
                {
                    f.WriteLine("t{1}: {0:N5} seconds", visits[i] / freq, i);
                }

                f.WriteLine("end: {0:N5} seconds", sw.ElapsedTicks / freq);
            }

            return 0;
        }
    }
}
