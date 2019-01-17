using Uragano.Abstractions;

namespace Sample
{
	[ServiceRoute("hello")]
	public interface IHelloService
	{
		[ServiceRoute("say")]
		string SayHello(string name);
	}
}
