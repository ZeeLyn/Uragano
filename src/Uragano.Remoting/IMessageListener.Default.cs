using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
    public class MessageListener : IMessageListener
    {
        public event ReceiveMessageHandler OnReceived;

        public async Task Received(TransportMessage<IServiceResult> message)
        {
            OnReceived?.Invoke(message);
            await Task.CompletedTask;
        }
    }
}
