using System;
using System.Threading.Tasks;
using Sample.Common;
using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{
    [ServiceDiscoveryName("RPC")]
    [ServiceRoute("hello")]
    [ClientClassInterceptor]

    public interface IHelloService : IService
    {
        [NonIntercept]
        [ClientMethodInterceptor]
        [CircuitBreaker(FallbackExecuteScript = "return new ResultModel{Message=\"fallback\"};", ScriptUsingNameSpaces = new[] { "Sample.Common" })]
        [Caching(Key = "customKey:{0}", ExpireSeconds = 30)]
        [ServiceRoute("say/async")]
        Task<ResultModel> SayHello(string name);

        [ServiceRoute("say/async/entity")]
        Task<ResultModel> SayHello(TestModel testModel);

        [ServiceRoute("say/task")]
        Task SayHello();

        [ServiceRoute("say/int")]
        Task<int> Age();

        Task<ResponseResult<string>> Test();

        Task Test(ResponseResult<string> r);
    }
}
