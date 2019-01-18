namespace Uragano.Abstractions
{
	public interface IServiceProxy
	{
		TService GetService<TService>() where TService : IService;
	}
}
