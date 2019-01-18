using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{

	[ServiceRoute("hello")]
	public interface IHelloService : IService
	{

		[ServiceRoute("say")]
		string SayHello(string name);
	}
}
