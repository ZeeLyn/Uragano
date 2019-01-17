using Microsoft.AspNetCore.Builder;
using Uragano.Abstractions;

namespace Uragano.Core
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseUraganoServer(this IApplicationBuilder applicationBuilder)
		{
			var serviceBuilder = (IServiceBuilder)applicationBuilder.ApplicationServices.GetService(typeof(IServiceBuilder));
			serviceBuilder.BuildServer();
			return applicationBuilder;
		}
	}
}
