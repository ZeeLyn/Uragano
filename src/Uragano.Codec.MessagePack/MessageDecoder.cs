using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Uragano.Abstractions;

namespace Uragano.Codec.MessagePack
{
    public class MessageDecoder<T> : MessageToMessageDecoder<IByteBuffer>
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            var len = message.ReadableBytes;
            var array = new byte[len];
            message.GetBytes(message.ReaderIndex, array, 0, len);
            output.Add(SerializerHelper.Deserialize<TransportMessage<T>>(array));
        }
    }
}
