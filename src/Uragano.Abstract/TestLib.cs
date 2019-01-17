using System;

namespace Uragano.Abstract
{
	public class TestLib : IDisposable
	{
		public void Exec()
		{
			Console.WriteLine("Exec-------------------->");
		}

		public void Dispose()
		{
			Console.WriteLine("Dispose-------------------->");
		}
	}
}
