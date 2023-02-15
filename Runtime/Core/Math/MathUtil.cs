namespace Hypnos.Core
{
    public static partial class MathUtil
    {
        public static int Combine(short hi, short lo)
        {
            return hi << 16 | (ushort)lo;
        }

        public static int RoundUpToPowerOfTwo(int i)
        {
            --i;
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            return i + 1;
        }
    }
}