namespace Uragano.Abstractions.Remoting
{
	public class TransportMessage<T>
	{
		public string Id { get; set; }

		public T Content { get; set; }
	}
}
