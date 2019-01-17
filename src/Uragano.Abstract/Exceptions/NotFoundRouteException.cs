using System;

namespace Uragano.Abstractions.Exceptions
{
	public class NotFoundRouteException : Exception
	{
		public NotFoundRouteException(string route) : base($"Route {route} not found.")
		{
		}
	}
}
