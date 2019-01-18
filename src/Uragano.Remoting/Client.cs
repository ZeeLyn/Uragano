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
			MessageListener.OnReceived += async (sender, message) =>
			{
				if (_resultCallbackTask.TryGetValue(message.Id, out var task))
				{
					task.SetResult(message.Content);
				}
				await Task.CompletedTask;
			};

		}

		public async Task<ResultMessage> SendAsync(InvokeMessage message)
		{
			var transportMessage = new TransportMessage<InvokeMessage>
			{
				Id = Guid.NewGuid().ToString(),
				Content = message
			};
			var callback = RegisterResultCallbackTaskAsync(transportMessage.Id);
			await Channel.WriteAndFlushAsync(transportMessage);
			return await callback;
		}




		private async Task<ResultMessage> RegisterResultCallbackTaskAsync(string id)
		{
			var task = new TaskCompletionSource<ResultMessage>();
			_resultCallbackTask.TryAdd(id, task);
			try
			{
				return await task.Task;
			}
			finally
			{
				_resultCallbackTask.TryRemove(id, out var t);
				t.TrySetCanceled();
			}
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
