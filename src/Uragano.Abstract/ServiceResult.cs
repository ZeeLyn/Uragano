using MessagePack;

namespace Uragano.Abstractions
{
    [MessagePackObject]
    public class ServiceResult : IServiceResult
    {
        public ServiceResult()
        {
        }

        public ServiceResult(object message, RemotingStatus status = RemotingStatus.Ok)
        {
            Result = message;
            Status = status;
        }

        [Key(0)]
        public object Result { get; set; }

        [Key(1)]
        public RemotingStatus Status { get; set; }
    }


}
