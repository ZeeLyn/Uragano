namespace Uragano.Abstractions
{
	public interface ITransportMessage<T>
	{
		string Id { get; set; }

		T Content { get; set; }
	}
}
