using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Codec.MessagePack;
using Uragano.Consul;
using Uragano.Core;
using Uragano.DynamicProxy;

namespace Sample.WebApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.AddUragano(config =>
			{
				config.AddConsul(Configuration.GetSection("Uragano:Consul:Client"));
				config.DependentServices(("RPC", "", ""));
				config.Option(UraganoOptions.Client_Node_Status_Refresh_Interval, TimeSpan.FromSeconds(10));
			});
			services.AddScoped<TestLib>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseMvc();
			app.UseUragano();
		}
	}
}
