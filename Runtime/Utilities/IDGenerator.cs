namespace Hypnos
{
    public static class IDGenerator
    {
        private static object locker = new object();
        private static ulong nowID = 1;
        private const ulong startId = 1;

        public static void Reset()
        {
            lock (locker)
            {
                nowID = startId;
            }
        }

        public static ulong Get()
        {
            lock (locker)
            {
                return nowID++;
            }
        }
    }
}