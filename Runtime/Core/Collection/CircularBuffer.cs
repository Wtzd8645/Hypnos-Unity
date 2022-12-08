using System;

namespace Morpheus.Core.Collection
{
    public class CircularBuffer<T>
    {
        private const int DefaultCapacity = 8;

        private readonly T[] buffer;
        private int headPos;
        private int tailPos;

        public int Count { get; private set; }
        public int Capacity => buffer.Length;

        public CircularBuffer(int capacity = DefaultCapacity)
        {
            if (capacity <= 0)
            {
                capacity = DefaultCapacity;
            }
            buffer = new T[capacity];
        }

        public void Clear()
        {
            Array.Clear(buffer, 0, buffer.Length);
            Count = 0;
            headPos = 0;
            tailPos = 0;
        }

        public void Enqueue(T item)
        {
            if (Count == 0)
            {
                ++Count;
                headPos = 0;
                tailPos = 0;
                buffer[0] = item;
                return;
            }

            if (Count < buffer.Length)
            {
                ++Count;
            }
            else
            {
                if (++headPos == buffer.Length)
                {
                    headPos = 0;
                }
            }

            if (++tailPos == buffer.Length)
            {
                tailPos = 0;
            }
            buffer[tailPos] = item;
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }

            --Count;
            int pos = headPos;
            if (++headPos == buffer.Length)
            {
                headPos = 0;
            }
            return buffer[pos];
        }

        public T Pop()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }

            --Count;
            int pos = tailPos;
            if (--tailPos < 0)
            {
                tailPos = buffer.Length - 1;
            }
            return buffer[pos];
        }
    }
}