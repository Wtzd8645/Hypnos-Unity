using System;
using UnityEngine;

namespace Blanketmen.Hypnos
{
    public class GameObjectPool
    {
        private readonly GameObject prefab;
        private readonly Transform root;

        private GameObject[] buffer;
        private int count;

        public int Capacity => buffer.Length;
        public int Count => count;

        public GameObjectPool(GameObject prefab, Transform root = null, int cap = 8, bool isPrefill = false)
        {
            this.prefab = prefab;
            this.root = root;

            buffer = new GameObject[cap];
            if (isPrefill)
            {
                Fill();
            }
        }

        public void Clear()
        {
            while (count-- > 0)
            {
                ResourceManager.Instance.Destroy(buffer[count]);
                buffer[count] = null;
            }
        }

        public void Fill()
        {
            while (count < buffer.Length)
            {
                buffer[count++] = ResourceManager.Instance.Create(prefab, root, false);
            }
        }

        public bool TryPush(GameObject obj)
        {
            if (count >= buffer.Length)
            {
                return false;
            }

            buffer[count++] = obj;
            return true;
        }

        public bool TryPop(out GameObject obj)
        {
            if (count == 0)
            {
                obj = null;
                return false;
            }

            obj = buffer[--count];
            return true;
        }

        public void Push(GameObject obj)
        {
            if (count >= buffer.Length)
            {
                GameObject[] newBuf = new GameObject[count * 2];
                Array.Copy(buffer, newBuf, count);
                buffer = newBuf;
            }

            buffer[count++] = obj;
        }

        public GameObject Pop()
        {
            return count > 0 ? buffer[--count] : ResourceManager.Instance.Create(prefab, root, false);
        }
    }

    public class GameObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform root;

        private T[] buffer;
        private int count;

        public int Capacity => buffer.Length;
        public int Count => count;

        public GameObjectPool(T prefab, Transform root = null, int cap = 8, bool isPrefill = false)
        {
            this.prefab = prefab;
            this.root = root;

            buffer = new T[cap];
            if (isPrefill)
            {
                Fill();
            }
        }

        public void Clear()
        {
            while (count-- > 0)
            {
                ResourceManager.Instance.Destroy(buffer[count].gameObject);
                buffer[count] = null;
            }
        }

        public void Fill()
        {
            while (count < buffer.Length)
            {
                buffer[count++] = ResourceManager.Instance.Create(prefab, root, false);
            }
        }

        public bool TryPush(T obj)
        {
            if (count >= buffer.Length)
            {
                return false;
            }

            buffer[count++] = obj;
            return true;
        }

        public bool TryPop(out T obj)
        {
            if (count == 0)
            {
                obj = null;
                return false;
            }

            obj = buffer[--count];
            return true;
        }

        public void Push(T obj)
        {
            if (count >= buffer.Length)
            {
                T[] newBuf = new T[count * 2];
                Array.Copy(buffer, newBuf, count);
                buffer = newBuf;
            }

            buffer[count++] = obj;
        }

        public T Pop()
        {
            return count > 0 ? buffer[--count] : ResourceManager.Instance.Create(prefab, root, false);
        }
    }
}