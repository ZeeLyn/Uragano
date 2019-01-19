using System.Threading.Tasks;

namespace Uragano.Remoting
{
	public interface IBootstrap
	{

		Task StartAsync();

		Task StopAsync();
	}
}
