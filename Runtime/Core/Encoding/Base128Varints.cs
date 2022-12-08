using System.IO;

namespace Morpheus.Core.Encoding
{
    public static class Base128Varints
    {
        public static unsafe void Write(uint value, byte* buffer, ref int offset)
        {
            while (value > 127u)
            {
                *(buffer + offset++) = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }
            *(buffer + offset++) = (byte)(value & 0x7F);
        }

        public static unsafe void Write(ulong value, byte* buffer, ref int offset)
        {
            while (value > 127ul)
            {
                *(buffer + offset++) = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }
            *(buffer + offset++) = (byte)(value & 0x7F);
        }

        public static unsafe uint ReadUInt32(byte* buffer, ref int offset)
        {
            int tmp = *(buffer + offset++);
            if (tmp < 128)
            {
                return (uint)tmp;
            }

            int result = tmp & 0x7f;
            if ((tmp = *(buffer + offset++)) < 128)
            {
                result |= tmp << 7;
                return (uint)result;
            }

            result |= (tmp & 0x7f) << 7;
            if ((tmp = *(buffer + offset++)) < 128)
            {
                result |= tmp << 14;
                return (uint)result;
            }

            result |= (tmp & 0x7f) << 14;
            if ((tmp = *(buffer + offset++)) < 128)
            {
                result |= tmp << 21;
                return (uint)result;
            }

            result |= (tmp & 0x7f) << 21;
            result |= (tmp = *(buffer + offset++)) << 28;
            if (tmp < 128)
            {
                return (uint)result;
            }

            // If larger than 32 bits, discard the upper 32 bits.
            for (int i = 0; i < 5; ++i)
            {
                if (*(buffer + offset++) < 128)
                {
                    return (uint)result;
                }
            }

            throw new IOException("Encountered a malformed varint.");
        }

        public static unsafe ulong ReadUInt64(byte* buffer, ref int offset)
        {
            ulong result = *(buffer + offset++);
            if (result < 128)
            {
                return result;
            }

            result &= 0x7f;
            int shift = 7;
            do
            {
                int tmp = *(buffer + offset++);
                result |= (ulong)(tmp & 0x7F) << shift;
                if (tmp < 128)
                {
                    return result;
                }
                shift += 7;
            }
            while (shift < 64);

            throw new IOException("Encountered a malformed varint.");
        }
    }
}