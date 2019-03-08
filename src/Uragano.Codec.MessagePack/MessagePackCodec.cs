using System;
using MessagePack;
using MessagePack.Resolvers;
using Uragano.Abstractions;

namespace Uragano.Codec.MessagePack
{
    public class MessagePackCodec : ICodec
    {
        public MessagePackCodec()
        {
            CompositeResolver.RegisterAndSetAsDefault(NativeDateTimeResolver.Instance, ContractlessStandardResolverAllowPrivate.Instance);
            MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolverAllowPrivate.Instance);
        }

        public byte[] Serialize<TData>(TData data)
        {
            return MessagePackSerializer.Typeless.Serialize(data);
        }

        public object Deserialize(byte[] data, Type type)
        {
            return data == null ? null : MessagePackSerializer.Typeless.Deserialize(data);
        }

        public T Deserialize<T>(byte[] data)
        {
            return data == null ? default : (T)MessagePackSerializer.Typeless.Deserialize(data);
        }

        public string ToJson<TData>(TData data)
        {
            return data == null ? default : MessagePackSerializer.ToJson(data);
        }
    }
}
