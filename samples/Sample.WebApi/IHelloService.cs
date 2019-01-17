using Uragano.Abstractions;


namespace Sample.WebApi
{
	[MyInterceptor3]
	[MyInterceptor4]
	[ServiceRoute("hello")]
	public interface IHelloService : IService
	{
		[MyInterceptor1]
		[MyInterceptor2]
		[ServiceRoute("say")]
		string SayHello(string name);
	}
}
