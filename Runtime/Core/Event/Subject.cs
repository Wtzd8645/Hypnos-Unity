using System;
using System.Collections.Generic;

namespace Morpheus.Core.Event
{
    // NOTE: There is GC overhead when delegates are merged.
    public class Subject<TKey>
    {
        protected Dictionary<TKey, Action> ParamlessHandlerMap;
        protected Dictionary<TKey, Delegate> HandlerMap;

        public Subject()
        {
            ParamlessHandlerMap = new Dictionary<TKey, Action>();
            HandlerMap = new Dictionary<TKey, Delegate>();
        }

        public Subject(IEqualityComparer<TKey> comparer)
        {
            ParamlessHandlerMap = new Dictionary<TKey, Action>(comparer);
            HandlerMap = new Dictionary<TKey, Delegate>(comparer);
        }

        public void UnregisterAll()
        {
            ParamlessHandlerMap.Clear();
            HandlerMap.Clear();
        }

        public void Register(TKey id, Action handler)
        {
            ParamlessHandlerMap.TryGetValue(id, out Action handlers);
            ParamlessHandlerMap[id] = handlers + handler;
        }

        public void Unregister(TKey id, Action handler)
        {
            if (ParamlessHandlerMap.TryGetValue(id, out Action handlers))
            {
                ParamlessHandlerMap[id] = handlers - handler;
            }
        }

        protected void Notify(TKey id)
        {
            ParamlessHandlerMap.TryGetValue(id, out Action handlers);
            handlers?.Invoke();
        }

        public void Register<T1>(TKey id, Action<T1> handler)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            if (del == null)
            {
                HandlerMap[id] = handler;
                return;
            }

            if (del is Action<T1> handlers)
            {
                HandlerMap[id] = handlers + handler;
                return;
            }

            throw new InvalidOperationException($"[Subject] Cannot register different types of handler functions in the same ID: {id}");
        }

        public void Unregister<T1>(TKey id, Action<T1> handler)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            if (del is Action<T1> handlers)
            {
                HandlerMap[id] = handlers - handler;
            }
        }

        protected void Notify<T1>(TKey id, T1 arg)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            (del as Action<T1>)?.Invoke(arg);
        }

        public void Register<T1, T2>(TKey id, Action<T1, T2> handler)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            if (del == null)
            {
                HandlerMap[id] = handler;
                return;
            }

            if (del is Action<T1, T2> handlers)
            {
                HandlerMap[id] = handlers + handler;
            }

            throw new InvalidOperationException($"[Subject] Cannot register different types of handler functions in the same ID: {id}");
        }

        public void Unregister<T1, T2>(TKey id, Action<T1, T2> handler)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            if (del is Action<T1, T2> handlers)
            {
                HandlerMap[id] = handlers - handler;
            }
        }

        protected void Notify<T1, T2>(TKey id, T1 arg1, T2 arg2)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            (del as Action<T1, T2>)?.Invoke(arg1, arg2);
        }

        public void Register<T1, T2, T3>(TKey id, Action<T1, T2, T3> handler)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            if (del == null)
            {
                HandlerMap[id] = handler;
                return;
            }

            if (del is Action<T1, T2, T3> handlers)
            {
                HandlerMap[id] = handlers + handler;
                return;
            }

            throw new InvalidOperationException($"[Subject] Cannot register different types of handler functions in the same ID: {id}");
        }

        public void Unregister<T1, T2, T3>(TKey id, Action<T1, T2, T3> handler)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            if (del is Action<T1, T2, T3> handlers)
            {
                HandlerMap[id] = handlers - handler;
            }
        }

        protected void Notify<T1, T2, T3>(TKey id, T1 arg1, T2 arg2, T3 arg3)
        {
            HandlerMap.TryGetValue(id, out Delegate del);
            (del as Action<T1, T2, T3>)?.Invoke(arg1, arg2, arg3);
        }
    }
}