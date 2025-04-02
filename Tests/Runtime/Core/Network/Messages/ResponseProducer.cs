using System;
using Blanketmen.Hypnos.Network;

namespace Blanketmen.Hypnos.Tests.Network
{
    public enum ResponseId : ushort
    {
        Echo = 65535
    }

    internal class ResponseProducer : IResponseProducer
    {
        public IResponse Produce(object source)
        {
            PacketBuffer buffer = source as PacketBuffer;
            ushort msgId = BitConverter.ToUInt16(buffer.final, buffer.offset);
            buffer.offset += NetworkDefs.MESSAGE_ID_LENGTH;

            IResponse response = msgId switch
            {
                (ushort)ResponseId.Echo => new EchoResponse(),
                _ => throw new NotImplementedException(msgId.ToString()),
            };
            response.Id = msgId;
            response.Unpack(buffer);
            return response;
        }
    }
}