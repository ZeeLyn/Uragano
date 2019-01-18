using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{

	[ServiceName("server1")]
	[ServiceRoute("hello")]
	public interface IHelloService : IService
	{

		[ServiceRoute("say")]
		ResultModel SayHello(string name);
	}
}
