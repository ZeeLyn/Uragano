namespace Uragano.Abstractions
{
	public interface IServiceProxy
	{
		TService GetService<TService>(string serviceName) where TService : IService;
	}
}
