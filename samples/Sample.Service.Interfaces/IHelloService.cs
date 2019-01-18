using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{

	[ServiceRoute("hello")]
	public interface IHelloService : IService
	{

		[ServiceRoute("say")]
		ResultModel SayHello(string name);
	}
}
