using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
	public class MessageListener : IMessageListener
	{
		public event ReceiveMessageHandler OnReceived;

		public async Task Received(IMessageSender sender, TransportMessage<ResultMessage> message)
		{
			OnReceived?.Invoke(sender, message);
			await Task.CompletedTask;
		}
	}
}
