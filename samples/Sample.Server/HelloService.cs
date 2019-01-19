using System.Threading.Tasks;
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


		public async Task<ResultModel> SayHello(string name)
		{
			TestLib.Exec();
			return await Task.FromResult(new ResultModel { Message = name });
		}
	}
}
