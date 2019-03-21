using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Service.Interfaces;
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
            //services.AddUragano(Configuration.GetSection("Uragano"));
            services.AddUragano(Configuration, builder =>
             {
                 builder.AddConsul();
                 builder.AddClient();
                 //builder.AddCircuitBreaker<CircuitBreakerEvent>(1000);
                 builder.AddCircuitBreaker<CircuitBreakerEvent>();
                 //builder.DependencyServices(("RPC", "", ""));
                 //builder.DependencyServices(Configuration.GetSection("Uragano:DependencyServices"));
                 //builder.Option(UraganoOptions.Client_Node_Status_Refresh_Interval, TimeSpan.FromSeconds(10));
                 builder.Options();
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
