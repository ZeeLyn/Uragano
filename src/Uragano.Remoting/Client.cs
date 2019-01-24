using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Uragano.Abstractions;

namespace Uragano.Remoting
{
	public class Client : IClient
	{
		private IChannel Channel { get; }
		private readonly ConcurrentDictionary<string, TaskCompletionSource<ResultMessage>> _resultCallbackTask =
			new ConcurrentDictionary<string, TaskCompletionSource<ResultMessage>>();

		private IMessageListener MessageListener { get; }

		public Client(IChannel channel, IMessageListener messageListener)
		{
			Channel = channel;
			MessageListener = messageListener;
			MessageListener.OnReceived += MessageListener_OnReceived;

		}

		private Task MessageListener_OnReceived(IMessageSender sender, TransportMessage<ResultMessage> message)
		{
			if (_resultCallbackTask.TryGetValue(message.Id, out var task))
			{
				task.SetResult(message.Body);
			}
			else
				Console.WriteLine("not found");
			return Task.CompletedTask;
		}

		public async Task<ResultMessage> SendAsync(InvokeMessage message)
		{
			var transportMessage = new TransportMessage<InvokeMessage>
			{
				Id = Guid.NewGuid().ToString(),
				Body = message
			};
			var task = new TaskCompletionSource<ResultMessage>(TaskCreationOptions.AttachedToParent);
			if (!_resultCallbackTask.TryAdd(transportMessage.Id, task)) throw new Exception("Failed to send.");
			var callback = Task.Run(async () =>
			{
				try
				{
					return await task.Task;
				}
				finally
				{
					_resultCallbackTask.TryRemove(transportMessage.Id, out var t);
					t.TrySetCanceled();
				}
			});
			await Channel.WriteAndFlushAsync(transportMessage);
			return await callback;

		}

		public void Dispose()
		{
			foreach (var task in _resultCallbackTask.Values)
			{
				task.TrySetCanceled();
			}

			Channel.DisconnectAsync();
			Channel.CloseAsync();
		}
	}
}
