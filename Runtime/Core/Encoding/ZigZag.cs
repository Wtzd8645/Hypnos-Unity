namespace Hypnos.Core.Encoding
{
    public static class ZigZag
    {
        public static uint Encode(int n)
        {
            return (uint)((n << 1) ^ (n >> 31));
        }

        public static ulong Encode(long n)
        {
            return (ulong)((n << 1) ^ (n >> 63));
        }

        public static int Decode(uint n)
        {
            return (int)(n >> 1) ^ -(int)(n & 1u);
        }

        public static long Decode(ulong n)
        {
            return (long)(n >> 1) ^ -(long)(n & 1u);
        }
    }
}