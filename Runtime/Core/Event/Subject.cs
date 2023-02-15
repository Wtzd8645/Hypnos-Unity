using System;
using System.Collections.Generic;

namespace Hypnos.Core.Event
{
    // NOTE: There is GC overhead when delegates are merged.
    public class Subject<TKey>
    {
        protected Dictionary<TKey, Action> paramlessHandlerMap;
        protected Dictionary<TKey, Delegate> handlerMap;

        public Subject()
        {
            paramlessHandlerMap = new Dictionary<TKey, Action>();
            handlerMap = new Dictionary<TKey, Delegate>();
        }

        public Subject(IEqualityComparer<TKey> comparer)
        {
            paramlessHandlerMap = new Dictionary<TKey, Action>(comparer);
            handlerMap = new Dictionary<TKey, Delegate>(comparer);
        }

        public void UnregisterAll()
        {
            paramlessHandlerMap.Clear();
            handlerMap.Clear();
        }

        public void Register(TKey id, Action handler)
        {
            paramlessHandlerMap.TryGetValue(id, out Action handlers);
            paramlessHandlerMap[id] = handlers + handler;
        }

        public void Unregister(TKey id, Action handler)
        {
            if (paramlessHandlerMap.TryGetValue(id, out Action handlers))
            {
                paramlessHandlerMap[id] = handlers - handler;
            }
        }

        protected void Notify(TKey id)
        {
            paramlessHandlerMap.TryGetValue(id, out Action handlers);
            handlers?.Invoke();
        }

        public void Register<T1>(TKey id, Action<T1> handler)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            if (del == null)
            {
                handlerMap[id] = handler;
                return;
            }

            if (del is Action<T1> handlers)
            {
                handlerMap[id] = handlers + handler;
                return;
            }

            throw new InvalidOperationException($"[Subject] Cannot register different types of handler functions in the same ID: {id}");
        }

        public void Unregister<T1>(TKey id, Action<T1> handler)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            if (del is Action<T1> handlers)
            {
                handlerMap[id] = handlers - handler;
            }
        }

        protected void Notify<T1>(TKey id, T1 arg)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            (del as Action<T1>)?.Invoke(arg);
        }

        public void Register<T1, T2>(TKey id, Action<T1, T2> handler)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            if (del == null)
            {
                handlerMap[id] = handler;
                return;
            }

            if (del is Action<T1, T2> handlers)
            {
                handlerMap[id] = handlers + handler;
            }

            throw new InvalidOperationException($"[Subject] Cannot register different types of handler functions in the same ID: {id}");
        }

        public void Unregister<T1, T2>(TKey id, Action<T1, T2> handler)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            if (del is Action<T1, T2> handlers)
            {
                handlerMap[id] = handlers - handler;
            }
        }

        protected void Notify<T1, T2>(TKey id, T1 arg1, T2 arg2)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            (del as Action<T1, T2>)?.Invoke(arg1, arg2);
        }

        public void Register<T1, T2, T3>(TKey id, Action<T1, T2, T3> handler)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            if (del == null)
            {
                handlerMap[id] = handler;
                return;
            }

            if (del is Action<T1, T2, T3> handlers)
            {
                handlerMap[id] = handlers + handler;
                return;
            }

            throw new InvalidOperationException($"[Subject] Cannot register different types of handler functions in the same ID: {id}");
        }

        public void Unregister<T1, T2, T3>(TKey id, Action<T1, T2, T3> handler)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            if (del is Action<T1, T2, T3> handlers)
            {
                handlerMap[id] = handlers - handler;
            }
        }

        protected void Notify<T1, T2, T3>(TKey id, T1 arg1, T2 arg2, T3 arg3)
        {
            handlerMap.TryGetValue(id, out Delegate del);
            (del as Action<T1, T2, T3>)?.Invoke(arg1, arg2, arg3);
        }
    }
}