using MessagePack;

namespace Uragano.Abstractions
{
    [MessagePackObject]
    public class TransportMessage<T> : ITransportMessage<T>
    {
        [Key(0)]
        public string Id { get; set; }

        [Key(1)]
        public T Body { get; set; }
    }
}
