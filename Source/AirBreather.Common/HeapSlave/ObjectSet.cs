﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime;

namespace HeapSlave
{
    class ObjectSet
    {
        struct Entry
        {
            public ulong High;
            public ulong Low;
            public int Index;
        }

        public ObjectSet(ClrHeap heap)
        {
            m_shift = IntPtr.Size == 4 ? 3 : 4;
            int count = heap.Segments.Count;

            m_data = new BitArray[count];
            m_entries = new Entry[count];
#if DEBUG
                ulong last = 0;
#endif

            for (int i = 0; i < count; ++i)
            {
                var seg = heap.Segments[i];
#if DEBUG
                    Debug.Assert(last < seg.Start);
                    last = seg.Start;
#endif

                m_data[i] = new BitArray(GetBitOffset(seg.Length));
                m_entries[i].Low = seg.Start;
                m_entries[i].High = seg.End;
                m_entries[i].Index = i;
            }
        }

        public void Add(ulong value)
        {
            if (value == 0)
            {
                m_zero = true;
                return;
            }

            int index = GetIndex(value);
            if (index == -1)
                return;

            int offset = GetBitOffset(value - m_entries[index].Low);

            m_data[index].Set(offset, true);
        }

        public bool Contains(ulong value)
        {
            if (value == 0)
                return m_zero;


            int index = GetIndex(value);
            if (index == -1)
                return false;

            int offset = GetBitOffset(value - m_entries[index].Low);

            return m_data[index][offset];
        }

        public int Count
        {
            get
            {
                // todo, this is nasty.
                int count = 0;
                foreach (var set in m_data)
                    foreach (bool bit in set)
                        if (bit) count++;

                return count;
            }
        }

        private int GetBitOffset(ulong offset)
        {
            Debug.Assert(offset < int.MaxValue);
            return GetBitOffset((int)offset);
        }

        private int GetBitOffset(int offset)
        {
            return offset >> m_shift;
        }

        private int GetIndex(ulong value)
        {
            int low = 0;
            int high = m_entries.Length - 1;

            while (low <= high)
            {
                int mid = (low + high) >> 1;
                if (value < m_entries[mid].Low)
                    high = mid - 1;
                else if (value > m_entries[mid].High)
                    low = mid + 1;
                else
                    return mid;
            }

            // Outside of the heap.
            return -1;
        }

        BitArray[] m_data;
        Entry[] m_entries;
        int m_shift;
        bool m_zero;
    }
}
