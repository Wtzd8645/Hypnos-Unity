using System;

namespace Morpheus
{
    public abstract class Singleton<T> : IDisposable where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        public static void ReleaseInstance()
        {
            Instance?.Dispose();
            Instance = null;
        }

        public static void CreateInstance()
        {
            Instance = Activator.CreateInstance(typeof(T), true) as T;
        }

        public virtual void Dispose() { }
    }
}