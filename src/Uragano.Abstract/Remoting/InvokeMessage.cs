namespace Uragano.Abstractions.Remoting
{
	public class InvokeMessage
	{
		public string Route { get; set; }

		public object[] Args { get; set; }
	}
}
