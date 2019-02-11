using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.Remoting
{


    public interface IClient : IDisposable
    {
        Task<IServiceResult> SendAsync(InvokeMessage message);
    }
}
