using System.Collections.Generic;

namespace Morpheus.Core.Memory
{
    public class LruCache<TKey, TValue> where TValue : class
    {
        private class Node
        {
            public TKey Key;
            public TValue Value;
            public Node Prev;
            public Node Next;
        }

        private const int DefaultCapacity = 8;

        private readonly Dictionary<TKey, Node> nodeMap;
        private readonly Node dummyHead = new Node();
        private readonly Node dummyTail = new Node();

        public int Capacity { get; private set; }
        public int Count { get; private set; }

        public LruCache(int capacity = DefaultCapacity)
        {
            nodeMap = new Dictionary<TKey, Node>(capacity);
            dummyHead.Next = dummyTail;
            dummyTail.Prev = dummyHead;
            Capacity = capacity > 0 ? capacity : DefaultCapacity;
        }

        public void Clear()
        {
            nodeMap.Clear();
            dummyHead.Next = dummyTail;
            dummyTail.Prev = dummyHead;
            Count = 0;
        }

        public TValue Get(TKey key)
        {
            nodeMap.TryGetValue(key, out Node node);
            if (node == null)
            {
                return null;
            }

            MoveToFront(node);
            return node.Value;
        }

        public void Put(TKey key, TValue value)
        {
            nodeMap.TryGetValue(key, out Node node);
            if (node != null)
            {
                node.Value = value;
                MoveToFront(node);
                return;
            }

            if (Count < Capacity)
            {
                ++Count;
                node = new Node();
            }
            else
            {
                node = dummyTail.Prev;
                node.Prev.Next = dummyTail;
                dummyTail.Prev = node.Prev;
                nodeMap.Remove(node.Key);
            }

            node.Key = key;
            node.Value = value;
            node.Prev = dummyHead;
            node.Next = dummyHead.Next;
            dummyHead.Next.Prev = node;
            dummyHead.Next = node;
            nodeMap[key] = node;
        }

        private void MoveToFront(Node node)
        {
            Node prev = node.Prev;
            Node next = node.Next;
            prev.Next = next;
            next.Prev = prev;
            node.Prev = dummyHead;
            node.Next = dummyHead.Next;
            dummyHead.Next.Prev = node;
            dummyHead.Next = node;
        }
    }
}