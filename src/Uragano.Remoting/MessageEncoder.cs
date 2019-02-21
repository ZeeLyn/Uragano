using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
    public class MessageEncoder<T> : MessageToByteEncoder<TransportMessage<T>>
    {
        private ICodec Codec { get; }

        public MessageEncoder(ICodec codec)
        {
            Codec = codec;
        }
        protected override void Encode(IChannelHandlerContext context, TransportMessage<T> message, IByteBuffer output)
        {
            var r = output.WriteBytes(Codec.Serialize(message));
        }
    }
}
