using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace AirBreather.Common.Caching
{
    // proof-of-concept stuff, really.
    public sealed class MemoryCachePlus
    {
        private readonly Guid id = Guid.NewGuid();
        private readonly List<object> stored = new List<object>();

        public void Put(object obj)
        {
            this.stored.Add(obj);
        }

        public string PrintHeap()
        {
            int pid;
            using (var curProc = Process.GetCurrentProcess())
            {
                pid = curProc.Id;
            }

            // proof-of-concept.  I regret nothing.
            var startInfo = new ProcessStartInfo(@"C:\Users\PC\src\AirBreather.Common\Source\AirBreather.Common\HeapSlave\bin\Debug\HeapSlave.exe", pid.ToString(CultureInfo.InvariantCulture) + " " + this.id.ToString("N"))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            StringBuilder sb = new StringBuilder();
            using (var proc = Process.Start(startInfo))
            {
                string line;
                while ((line = proc.StandardOutput.ReadLine()) != null)
                {
                    string[] s = line.Split('_');
                    int idx = Int32.Parse(s[0], NumberStyles.None, CultureInfo.InvariantCulture);
                    bool shared = Int32.Parse(s[1], NumberStyles.None, CultureInfo.InvariantCulture) != 0;
                    sb.AppendFormat(CultureInfo.InvariantCulture, "Element at index {0} is ", idx);
                    if (!shared)
                    {
                        sb.Append("not ");
                    }

                    sb.AppendLine("shared.");
                }
            }

            return sb.ToString();
        }
    }
}
