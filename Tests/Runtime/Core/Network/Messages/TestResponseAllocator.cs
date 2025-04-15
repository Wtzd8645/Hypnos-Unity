using System;
using Blanketmen.Hypnos.Network;

namespace Blanketmen.Hypnos.Tests.Network
{
    public enum ResponseId : ushort
    {
        Echo = 65535
    }

    internal class TestResponseAllocator : IResponseAllocator
    {
        public IResponse Acquire(byte gid, ushort id)
        {
            throw new NotImplementedException();
        }

        public IResponse Acquire(Span<byte> buf)
        {
            ushort msgId = BitConverter.ToUInt16(buf);
            IResponse response = msgId switch
            {
                (ushort)ResponseId.Echo => new EchoResponse(),
                _ => throw new NotImplementedException(msgId.ToString()),
            };

            response.Id = msgId;
            response.Unpack(buf.Slice(NetworkDefs.MESSAGE_ID_LENGTH));
            return response;
        }

        public void Release(IResponse resp)
        {

        }
    }
}