using System;
using System.Net;
using System.Threading.Tasks;


namespace Sample.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var ip = new IPEndPoint(IPAddress.Parse("192.168.1.129"), 111);
			System.Console.WriteLine(ip.Address.ToString());

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
