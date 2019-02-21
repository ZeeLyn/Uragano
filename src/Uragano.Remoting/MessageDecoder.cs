using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
    public class MessageDecoder<T> : MessageToMessageDecoder<IByteBuffer>
    {
        private ICodec Codec { get; }


        public MessageDecoder(ICodec codec)
        {
            Codec = codec;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            var len = message.ReadableBytes;
            var array = new byte[len];
            message.GetBytes(message.ReaderIndex, array, 0, len);
            //output.Add(SerializerHelper.Deserialize<TransportMessage<T>>(array));
            output.Add(Codec.Deserialize(array, typeof(T)));
        }
    }
}
