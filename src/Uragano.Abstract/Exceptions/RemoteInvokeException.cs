using System;

namespace Uragano.Abstractions.Exceptions
{
	public class RemoteInvokeException : Exception
	{
		public RemoteInvokeException(string route, string message) : base($"Remote call exception(route:{route}):{message}")
		{
		}
	}
}
