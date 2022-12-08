using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Morpheus.Core.Memory
{
    public class ObjectPool<T> where T : class
    {
        private const BindingFlags CtorFlags = BindingFlags.Public | BindingFlags.Instance;
        private const int DefaultCapacity = 8;

        private readonly Func<T> constructor;
        private T[] buffer;

        public int Capacity => buffer.Length;
        public int Count { get; private set; }

        public ObjectPool(Func<T> ctor = null, int capacity = DefaultCapacity, bool isPrefilled = false)
        {
            if (ctor == null)
            {
                ConstructorInfo ctorInfo = typeof(T).GetConstructor(CtorFlags, null, Type.EmptyTypes, null);
                NewExpression newExpr = Expression.New(ctorInfo, (IEnumerable<Expression>)null);
                constructor = Expression.Lambda<Func<T>>(newExpr).Compile();
            }
            else
            {
                constructor = ctor;
            }
            buffer = new T[capacity > 0 ? capacity : DefaultCapacity];

            if (isPrefilled)
            {
                Fill();
            }
        }

        public void Clear()
        {
            Array.Clear(buffer, 0, Count);
            Count = 0;
        }

        public void Fill()
        {
            while (Count < buffer.Length)
            {
                buffer[Count++] = constructor();
            }
        }

        public T Pop()
        {
            return Count > 0 ? buffer[--Count] : null;
        }

        public T ForcePop()
        {
            return Count > 0 ? buffer[--Count] : constructor();
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