namespace Morpheus
{
    public static class IdGenerator
    {
        private static object locker = new object();
        private static ulong nowId = 1;
        private const ulong StartId = 1;

        public static void Reset()
        {
            lock (locker)
            {
                nowId = StartId;
            }
        }

        public static ulong Get()
        {
            lock (locker)
            {
                return nowId++;
            }
        }
    }
}