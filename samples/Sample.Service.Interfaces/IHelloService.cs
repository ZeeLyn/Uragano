using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Sample.Service.Interfaces
{
    [ClientInterceptor_1_]
    [ClientInterceptor_2_]
    [ServiceDiscoveryName("RPC")]
    [ServiceRoute("hello")]
    public interface IHelloService : IService
    {

        [ClientMethodInterceptor_1_]
        [ClientMethodInterceptor_2_]
        [ServiceRoute("say/async")]
        Task<ResultModel> SayHello(string name);

        [ServiceRoute("say/task")]
        Task SayHello();

    }
}
