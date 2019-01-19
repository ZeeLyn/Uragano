namespace Uragano.Abstractions.ServiceDiscovery
{
	public interface IServiceRegisterConfiguration
	{
		string ServiceId { get; set; }
		int? Weight { get; set; }
	}
}
