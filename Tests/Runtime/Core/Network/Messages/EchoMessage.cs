using Blanketmen.Hypnos.Network;
using System;

namespace Blanketmen.Hypnos.Tests.Network
{
    public class EchoRequest : IRequest
    {
        public ushort gid;
        public ushort id;
        public int a;
        public string b;
        public double c;

        private ushort offset;
        
        public ushort Gid
        {
            get => gid;
        }

        public ushort Id
        {
            get => id;
        }

        public EchoRequest()
        {
            id = 65535;
        }

        public unsafe ushort Pack(Span<byte> buf)
        {
            fixed (byte* ptr = buf)
            {
                *(ushort*)(ptr + offset) = Id;
                offset += sizeof(ushort);

                *(int*)(ptr + offset) = a;
                offset += sizeof(int);

                fixed (char* strPtr = b)
                {
                    ushort strLen = (ushort)NetworkDefs.StringEncoder.GetByteCount(strPtr, b.Length);
                    *(int*)(ptr + offset) = strLen;
                    offset += sizeof(int);

                    NetworkDefs.StringEncoder.GetBytes(strPtr, b.Length, ptr + offset, strLen);
                    offset += strLen;
                }

                *(double*)(ptr + offset) = c;
                offset += sizeof(double);
            }
            return offset;
        }
    }

    public class EchoResponse : IResponse
    {
        public byte gid;
        public ushort id;
        public int a;
        public string b;
        public double c;

        private int offset;

        public byte Gid
        {
            get => gid;
            set => gid = value;
        }

        public ushort Id
        {
            get => id;
            set => id = value;
        }

        public unsafe void Unpack(Span<byte> buf)
        {
            fixed (byte* ptr = buf)
            {
                a = *(int*)(ptr + offset);
                offset += sizeof(int);

                fixed (char* strPtr = b)
                {
                    int strLen = *(int*)(ptr + offset);
                    offset += sizeof(int);

                    b = NetworkDefs.StringEncoder.GetString(ptr + offset, strLen);
                    offset += strLen;
                }

                c = *(double*)(ptr + offset);
                offset += sizeof(double);
            }
        }
    }
}