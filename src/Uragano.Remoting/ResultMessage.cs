using MessagePack;
using Uragano.Abstractions;

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
        public RemotingStatus Status { get; set; } = RemotingStatus.Ok;
    }


}
