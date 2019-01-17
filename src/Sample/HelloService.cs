namespace Sample
{
	public class HelloService : IHelloService
	{
		public string SayHello(string name)
		{
			return "Hello " + name;
		}
	}
}
