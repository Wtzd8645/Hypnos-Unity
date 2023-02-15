using System.Collections.Generic;

namespace Hypnos.Core.Memory
{
    public class LruCache<TKey, TValue> where TValue : class
    {
        private class Node
        {
            public TKey key;
            public TValue value;
            public Node prev;
            public Node next;
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
            dummyHead.next = dummyTail;
            dummyTail.prev = dummyHead;
            Capacity = capacity > 0 ? capacity : DefaultCapacity;
        }

        public void Clear()
        {
            nodeMap.Clear();
            dummyHead.next = dummyTail;
            dummyTail.prev = dummyHead;
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
            return node.value;
        }

        public void Put(TKey key, TValue value)
        {
            nodeMap.TryGetValue(key, out Node node);
            if (node != null)
            {
                node.value = value;
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
                node = dummyTail.prev;
                node.prev.next = dummyTail;
                dummyTail.prev = node.prev;
                nodeMap.Remove(node.key);
            }

            node.key = key;
            node.value = value;
            node.prev = dummyHead;
            node.next = dummyHead.next;
            dummyHead.next.prev = node;
            dummyHead.next = node;
            nodeMap[key] = node;
        }

        private void MoveToFront(Node node)
        {
            Node prev = node.prev;
            Node next = node.next;
            prev.next = next;
            next.prev = prev;
            node.prev = dummyHead;
            node.next = dummyHead.next;
            dummyHead.next.prev = node;
            dummyHead.next = node;
        }
    }
}