using MessagePack;

namespace Uragano.Remoting
{
	[MessagePackObject]
	public class ResultMessage
	{
		public ResultMessage(object message)
		{
			Result = message;
		}
		[Key(0)]
		public object Result { get; }

		[Key(1)]
		public string Type { get; set; }

		[Key(2)] public RemoteStatus Status { get; set; } = RemoteStatus.Ok;
	}

	public enum RemoteStatus
	{
		Ok = 200,
		Error = 500
	}
}
