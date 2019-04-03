using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Common;
using Uragano.Abstractions;
using Uragano.Caching.Redis;
using Uragano.Consul;
using Uragano.Core;
using Uragano.Logging.Exceptionless;
using Uragano.Remoting.LoadBalancing;
using Uragano.ZooKeeper;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Sample.Server
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
            services.AddUragano(Configuration, builder =>
             {

                 builder.AddClient(LoadBalancing.WeightedRandom);
                 builder.AddServer();
                 //builder.AddZooKeeper();
                 builder.AddConsul();
                 builder.AddClientGlobalInterceptor<ClientGlobalInterceptor>();
                 builder.AddServerGlobalInterceptor<ServerGlobalInterceptor>();
                 builder.AddExceptionlessLogger();
                 //builder.AddLog4NetLogger();
                 //builder.AddNLogLogger();
                 builder.AddRedisPartitionCaching();
                 //builder.AddRedisCaching();
                 //builder.AddMemoryCaching();
                 builder.AddOption(UraganoOptions.Remoting_Invoke_CancellationTokenSource_Timeout, TimeSpan.FromSeconds(10));
                 builder.AddOptions();
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
