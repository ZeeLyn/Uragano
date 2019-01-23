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

		[ServiceRoute("say")]
		Task<ResultModel> SayHello(string name, int age);

		Task SayHello();

		void SayHello(string name);

		int SayHello(string name, List<string> list);
	}
}
