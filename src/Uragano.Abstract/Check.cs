using System;

namespace Uragano.Abstractions
{

	public class Check
	{
		public static T NotNull<T>(T value, string name) where T : class
		{
			if (value == null)
				throw new ArgumentNullException(name);
			return value;
		}
	}
}
