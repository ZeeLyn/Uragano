using System;

namespace Uragano.Abstractions.Exceptions
{
    public class NotFoundNodeException : Exception
    {
        public string ServiceName { get; }

        public NotFoundNodeException(string serviceName) : base($"Service {serviceName} did not found available nodes.")
        {
            ServiceName = serviceName;
        }
    }
}
