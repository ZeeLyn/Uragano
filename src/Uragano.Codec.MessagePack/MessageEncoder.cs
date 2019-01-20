using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Uragano.Abstractions;

namespace Uragano.Codec.MessagePack
{
	public class MessageEncoder<T> : MessageToByteEncoder<TransportMessage<T>>
	{
		protected override void Encode(IChannelHandlerContext context, TransportMessage<T> message, IByteBuffer output)
		{
			output.WriteBytes(SerializerHelper.Serialize(message));
		}
	}
}
