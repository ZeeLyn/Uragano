using System;

namespace Uragano.Abstractions
{
    public interface ICodec
    {
        byte[] Serialize<TData>(TData data);

        object Deserialize(byte[] data, Type type);

        T Deserialize<T>(byte[] data);
    }
}
