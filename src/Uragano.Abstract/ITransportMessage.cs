namespace Uragano.Abstractions
{
	public interface ITransportMessage<T>
	{
		string Id { get; set; }

		T Body { get; set; }
	}
}
