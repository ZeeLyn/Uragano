namespace Uragano.Abstractions
{
	public class TransportMessage<T> : ITransportMessage<T>
	{
		public string Id { get; set; }

		public T Body { get; set; }
	}
}
