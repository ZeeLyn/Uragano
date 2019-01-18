using DotNetty.Buffers;
using DotNetty.Codecs;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions.Remoting;

namespace Uragano.Codec.MessagePack
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection UseMessagePackCodec(this IServiceCollection serviceCollection)
		{
			////serviceCollection.AddSingleton<MessageToByteEncoder<TransportMessage>, MessageEncoder>();
			////serviceCollection.AddSingleton<MessageToMessageDecoder<IByteBuffer>, MessageDecoder>();
			return serviceCollection;
		}
	}
}
