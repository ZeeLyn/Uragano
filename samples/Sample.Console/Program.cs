using System;
using System.Net;
using System.Threading.Tasks;


namespace Sample.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			///var ip = new IPEndPoint();
			//var r = IPAddress.Parse("localhost");
			try
			{
				var dns = new DnsEndPoint("localhost", 80);
				System.Console.WriteLine(dns);
			}
			catch (Exception e)
			{
				var r = e;
			}

			System.Console.ReadKey();
		}
	}

	public class A
	{
		public object Data { get; set; }
	}

	public class B
	{
		public string Name { get; set; }

		public int Age { get; set; }
	}
}
