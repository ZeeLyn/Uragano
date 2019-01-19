using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uragano.Abstractions;
using Uragano.Codec.MessagePack;
using Uragano.Core;

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
				//config.AddServer("127.0.0.1", 5001);

			});
			services.AddUraganoClient();
			services.AddScoped<TestLib>();
			services.UseMessagePackCodec();
			//services.AddScoped<IInterceptor, Test1Interceptor>();
			//services.AddScoped<IInterceptor, Test2Interceptor>();
			//AutofacContainer.Populate(services);
			//AutofacContainer.Build();
			//return new AutofacServiceProvider(AutofacContainer.GetContainer());
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}


			app.UseMvc();
			app.UseUraganoClient();
		}
	}
}
