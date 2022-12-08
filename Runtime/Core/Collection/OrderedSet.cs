using System.Collections;
using System.Collections.Generic;

namespace Morpheus.Core.Collection
{
    public class OrderedSet<T> : IEnumerable<T>
    {
        private readonly HashSet<T> hashSet;
        private readonly LinkedList<T> linkedList;

        public int Count => hashSet.Count;

        public OrderedSet() : this(EqualityComparer<T>.Default) { }

        public OrderedSet(IEqualityComparer<T> comparer)
        {
            hashSet = new HashSet<T>(comparer);
            linkedList = new LinkedList<T>();
        }

        public void Clear()
        {
            hashSet.Clear();
            linkedList.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return linkedList.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return linkedList.GetEnumerator();
        }

        public bool Contains(T item)
        {
            return item != null && hashSet.Contains(item);
        }

        public bool Add(T item)
        {
            if (hashSet.Add(item))
            {
                linkedList.AddLast(item);
                return true;
            }
            return false;
        }

        public bool Remove(T item)
        {
            if (hashSet.Remove(item))
            {
                linkedList.Remove(item);
                return true;
            }
            return false;
        }
    }
}