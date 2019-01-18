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


		[Key(1)] public RemotingStatus Status { get; set; } = RemotingStatus.Ok;
	}

	public enum RemotingStatus
	{
		Ok = 200,
		NotFound = 404,
		Error = 500
	}
}
