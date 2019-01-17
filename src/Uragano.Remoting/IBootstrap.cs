using System.Threading.Tasks;

namespace Uragano.Remoting
{
	public interface IBootstrap
	{

		Task StartAsync(string host, int port);

		Task StopAsync();
	}
}
