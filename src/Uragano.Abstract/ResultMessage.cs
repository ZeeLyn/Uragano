using MessagePack;
using MessagePack.Formatters;

namespace Uragano.Abstractions
{
    [MessagePackObject]
    public class ResultMessage : IServiceResult
    {
        public ResultMessage()
        {
        }

        public ResultMessage(object message, RemotingStatus status = RemotingStatus.Ok)
        {
            Result = message;
            Status = status;
        }

        [Key(0), MessagePackFormatter(typeof(TypelessFormatter))]
        public object Result { get; set; }

        [Key(1)]
        public RemotingStatus Status { get; set; }
    }


}
