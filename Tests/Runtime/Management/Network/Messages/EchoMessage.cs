namespace Blanketmen.Hypnos.Tests.Network
{
    public class EchoRequest : IRequest
    {
        public ushort msgId;
        public int a;
        public string b;
        public double c;

        private int offset;

        public ushort Id
        {
            get => msgId;
        }

        public EchoRequest()
        {
            msgId = 65535;
        }

        public unsafe int Pack(PacketBuffer result)
        {
            fixed (byte* buf = &result.final[result.offset])
            {
                *(ushort*)(buf + offset) = msgId;
                offset += sizeof(ushort);

                *(int*)(buf + offset) = a;
                offset += sizeof(int);

                fixed (char* strPtr = b)
                {
                    int strLen = NetworkManager.StringEncoder.GetByteCount(strPtr, b.Length);
                    *(int*)(buf + offset) = strLen;
                    offset += sizeof(int);

                    NetworkManager.StringEncoder.GetBytes(strPtr, b.Length, buf + offset, strLen);
                    offset += strLen;
                }

                *(double*)(buf + offset) = c;
                offset += sizeof(double);
            }
            return offset;
        }
    }

    public class EchoResponse : IResponse
    {
        public ushort msgId;
        public int a;
        public string b;
        public double c;

        private int offset;

        public ushort Id
        {
            get => msgId;
            set => msgId = value;
        }

        public unsafe void Unpack(PacketBuffer source)
        {
            fixed (byte* buf = &source.final[source.offset])
            {
                a = *(int*)(buf + offset);
                offset += sizeof(int);

                fixed (char* strPtr = b)
                {
                    int strLen = *(int*)(buf + offset);
                    offset += sizeof(int);

                    b = NetworkManager.StringEncoder.GetString(buf + offset, strLen);
                    offset += strLen;
                }

                c = *(double*)(buf + offset);
                offset += sizeof(double);
            }
        }
    }
}