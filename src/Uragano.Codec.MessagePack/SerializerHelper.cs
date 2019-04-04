using MP = MessagePack;
using MessagePack.Resolvers;

namespace Uragano.Codec.MessagePack
{
    public class SerializerHelper
    {
        static SerializerHelper()
        {
            CompositeResolver.RegisterAndSetAsDefault(NativeDateTimeResolver.Instance, ContractlessStandardResolverAllowPrivate.Instance);
            MP.MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolverAllowPrivate.Instance);
        }

        public static byte[] Serialize<T>(T data)
        {
            return MP.MessagePackSerializer.Typeless.Serialize(data);
        }

        public static object Deserialize(byte[] data)
        {
            return data == null || data.Length==0 ? null : MP.MessagePackSerializer.Typeless.Deserialize(data);
        }

        public static T Deserialize<T>(byte[] data)
        {
            return data == null || data.Length == 0 ? default : (T)Deserialize(data);
        }
    }
}
