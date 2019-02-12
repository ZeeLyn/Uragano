using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
    public delegate void ReceiveMessageHandler(TransportMessage<IServiceResult> message);
    public interface IMessageListener
    {
        event ReceiveMessageHandler OnReceived;

        Task Received(TransportMessage<IServiceResult> message);
    }
}
