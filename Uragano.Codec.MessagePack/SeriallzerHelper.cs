using System;
using MessagePack;
using MessagePack.Resolvers;

namespace Uragano.Codec.MessagePack
{
	public class SerializerHelper
	{
		static SerializerHelper()
		{
			CompositeResolver.RegisterAndSetAsDefault(NativeDateTimeResolver.Instance, ContractlessStandardResolverAllowPrivate.Instance);
			MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolverAllowPrivate.Instance);
		}

		public static byte[] Serialize<T>(T data)
		{
			return MessagePackSerializer.Serialize(data);
		}

		public static object Deserialize(byte[] data, Type type)
		{
			return data == null ? null : MessagePackSerializer.NonGeneric.Deserialize(type, data);
		}

		public static T Deserialize<T>(byte[] data)
		{
			return data == null ? default : (T)Deserialize(data, typeof(T));
		}
	}
}
