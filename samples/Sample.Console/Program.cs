using System;
using System.Threading.Tasks;
using Uragano.Codec.MessagePack;

namespace Sample.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var result = new A
			{
				Data = new B
				{
					Name = "OWEN",
					Age = 10
				}
			};
			var bytes = SerializerHelper.Serialize(result);

			var r = SerializerHelper.Deserialize<A>(bytes);

			System.Console.WriteLine(r);
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
