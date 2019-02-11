using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.Remoting
{


    public interface IClient : IDisposable
    {
        Task<ResultMessage> SendAsync(InvokeMessage message);
    }
}
