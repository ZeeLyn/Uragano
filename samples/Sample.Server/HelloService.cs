using System.Collections.Generic;
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

		public Task SayHello()
		{
			throw new System.NotImplementedException();
		}

		public void SayHello(string name, int age)
		{
			throw new System.NotImplementedException();
		}

		public ResultModel SayHelloVoid(string name)
		{
			return new ResultModel { Message = name };
		}
	}
}
