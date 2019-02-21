using System;
using System.Threading.Tasks;
using Sample.Common;
using Sample.Service.Interfaces;
using Uragano.Abstractions;

namespace Sample.Server
{

    [ServerInterceptor]
    public class HelloService : IHelloService
    {

        public async Task<ResultModel> SayHello(string name)
        {
            // await Task.Delay(2000);
            return await Task.FromResult(new ResultModel { Message = name });
        }

        public async Task<ResultModel> SayHello(TestModel testModel)
        {
            return await Task.FromResult(new ResultModel
            {
                Message = "Rec " + testModel.Name
            });
        }

        public async Task SayHello()
        {

        }

        public async Task<int> Age()
        {
            //await Task.Delay(2000);
            return await Task.FromResult(18);
        }
    }
}
