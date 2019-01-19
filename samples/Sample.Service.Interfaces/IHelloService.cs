using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{
	[MyInterceptor1]
	[ServiceName("server1")]
	[ServiceRoute("hello")]
	public interface IHelloService : IService
	{

		[ServiceRoute("say")]
		Task<ResultModel> SayHello(string name);
	}
}
