using Sample.Service.Interfaces;
using Uragano.Abstractions;

namespace Sample.Server
{

	public class HelloService : IHelloService
	{
		private TestLib TestLib { get; }

		public HelloService(TestLib testLib)
		{
			TestLib = testLib;
		}


		public string SayHello(string name)
		{
			TestLib.Exec();
			return "Hello " + name;
		}
	}
}
