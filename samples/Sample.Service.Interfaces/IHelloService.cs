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

		[ServiceRoute("say/void")]
		void SayHello(string name, int age);

		[ServiceRoute("say/void/return")]
		ResultModel SayHelloVoid(string name);
	}
}
