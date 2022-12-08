using System;
using UnityEngine;

namespace Morpheus.Resource
{
    public class GameObjectPool
    {
        private const int DefaultCapacity = 8;

        private readonly GameObject prefab;
        private readonly Transform root;
        private GameObject[] buffer;

        public int Capacity => buffer.Length;
        public int Count { get; private set; }

        public GameObjectPool(GameObject prefab, Transform root = null, int capacity = DefaultCapacity, bool isPrefilled = false)
        {
            this.prefab = prefab;
            this.root = root;
            buffer = new GameObject[capacity > 0 ? capacity : DefaultCapacity];

            if (isPrefilled)
            {
                Fill();
            }
        }

        public void Clear()
        {
            while (Count > 0)
            {
                --Count;
                ResourceManager.Instance.Destroy(buffer[Count]);
                buffer[Count] = null;
            }
        }

        public void Fill()
        {
            while (Count < buffer.Length)
            {
                buffer[Count++] = ResourceManager.Instance.Create(prefab, root, false);
            }
        }

        public GameObject Pop()
        {
            return Count > 0 ? buffer[--Count] : null;
        }

        public GameObject ForcePop()
        {
            return Count > 0 ? buffer[--Count] : ResourceManager.Instance.Create(prefab, root, false);
        }

        public void Push(GameObject obj)
        {
            if (Count >= buffer.Length)
            {
                return;
            }
            buffer[Count++] = obj;
        }

        public void ForcePush(GameObject obj)
        {
            if (Count >= buffer.Length)
            {
                GameObject[] newBuf = new GameObject[Count * 2];
                Array.Copy(buffer, newBuf, Count);
                buffer = newBuf;
            }
            buffer[Count++] = obj;
        }
    }

    public class GameObjectPool<T> where T : Component
    {
        private const int DefaultCapacity = 8;

        private readonly T prefab;
        private readonly Transform root;
        private T[] buffer;

        public int Capacity => buffer.Length;
        public int Count { get; private set; }

        public GameObjectPool(T prefab, Transform root = null, int capacity = DefaultCapacity, bool isPrefilled = false)
        {
            this.prefab = prefab;
            this.root = root;
            buffer = new T[capacity > 0 ? capacity : DefaultCapacity];

            if (isPrefilled)
            {
                Fill();
            }
        }

        public void Clear()
        {
            while (Count > 0)
            {
                --Count;
                ResourceManager.Instance.Destroy(buffer[Count].gameObject);
                buffer[Count] = null;
            }
        }

        public void Fill()
        {
            while (Count < buffer.Length)
            {
                buffer[Count++] = ResourceManager.Instance.Create(prefab, root, false);
            }
        }

        public T Pop()
        {
            return Count > 0 ? buffer[--Count] : null;
        }

        public T ForcePop()
        {
            return Count > 0 ? buffer[--Count] : ResourceManager.Instance.Create(prefab, root, false);
        }

        public void Push(T obj)
        {
            if (Count >= buffer.Length)
            {
                return;
            }
            buffer[Count++] = obj;
        }

        public void ForcePush(T obj)
        {
            if (Count >= buffer.Length)
            {
                T[] newBuf = new T[Count * 2];
                Array.Copy(buffer, newBuf, Count);
                buffer = newBuf;
            }
            buffer[Count++] = obj;
        }
    }
}