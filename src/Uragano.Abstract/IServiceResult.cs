using MessagePack;

namespace Uragano.Abstractions
{
    [Union(0, typeof(ServiceResult))]
    public interface IServiceResult
    {
        [Key(0)]
        object Result { get; set; }

        [Key(1)]
        RemotingStatus Status { get; set; }
    }

    public enum RemotingStatus
    {
        Ok = 200,
        NotFound = 404,
        Error = 500,
        Unauthorized = 401,
        Forbidden = 403,
        Timeout = 504
    }
}
