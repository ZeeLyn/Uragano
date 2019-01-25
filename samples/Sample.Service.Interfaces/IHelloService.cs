using System.Collections.Generic;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{
	[MyInterceptor1]
	[ServiceDiscoveryName("TestServer")]
	[ServiceRoute("hello")]
	public interface IHelloService : IService
	{

		[ServiceRoute("say/async")]
		Task<ResultModel> SayHello(string name);

		[ServiceRoute("say/task")]
		Task SayHello();

	}
}
