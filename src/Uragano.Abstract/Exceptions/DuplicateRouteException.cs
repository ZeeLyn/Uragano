using System;

namespace Uragano.Abstractions.Exceptions
{
	public class DuplicateRouteException : Exception
	{
		public DuplicateRouteException(string route) : base($"Duplicate route {route}.")
		{

		}
	}
}
