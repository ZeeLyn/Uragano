using System;

namespace Uragano.Abstractions.Exceptions
{
    public class RemoteInvokeException : Exception
    {
        public RemoteInvokeException(string route, string message, RemotingStatus status) : base($"Remote call exception(route:{route}):{message}")
        {
            Route = route;
            RemotingStatus = status;
        }

        public string Route { get; }

        public RemotingStatus RemotingStatus { get; }
    }
}
