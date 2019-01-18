using System;
using System.Threading.Tasks;
using Uragano.Abstractions.Remoting;

namespace Uragano.Remoting
{
	public class MessageListener : IMessageListener
	{
		public event ReceiveMessageHandler OnReceived;
		public async Task Received(IMessageSender sender, TransportMessage<ResultMessage> message)
		{
			if (OnReceived == null)
				return;
			await OnReceived(sender, message);
		}
	}
}
