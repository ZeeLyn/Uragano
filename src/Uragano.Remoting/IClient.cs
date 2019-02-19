using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.Remoting
{


    public interface IClient
    {
        Task<IServiceResult> SendAsync(IInvokeMessage message);

        Task DisconnectAsync();
    }
}
