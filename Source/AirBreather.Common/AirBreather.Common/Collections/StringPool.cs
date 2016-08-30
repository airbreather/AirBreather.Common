using System.Collections.Generic;

namespace AirBreather.Common.Collections
{
    public sealed class StringPool
    {
        private readonly Dictionary<string, string> pool = new Dictionary<string, string>();

        public string Pool(string val)
        {
            string result;
            lock (this.pool)
            {
                if (!this.pool.TryGetValue(val, out result))
                {
                    result = this.pool[val] = val;
                }
            }

            return result;
        }

        public string IsPooled(string val)
        {
            lock (this.pool)
            {
                this.pool.TryGetValue(val, out val);
            }

            return val;
        }

        public void Clear()
        {
            lock (this.pool)
            {
                this.pool.Clear();
            }
        }
    }
}
