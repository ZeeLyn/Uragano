using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.Common;
using Sample.Service.Interfaces;
using Uragano.Abstractions;
using Uragano.Caching.Redis;
using Uragano.Codec.MessagePack;
using Uragano.Consul;
using Uragano.Core;

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
            services.AddUragano(config =>
            {
                config.IsDevelopment(true);
                config.AddClient();
                config.AddServer(Configuration.GetSection("Uragano:Server"));
                config.AddConsul(Configuration.GetSection("Uragano:Consul:Client"),
                    Configuration.GetSection("Uragano:Consul:Service"));
                config.AddClientGlobalInterceptor<ClientGlobal_1_Interceptor>();
                //config.AddClientGlobalInterceptor<ClientGlobal_2_Interceptor>();
                //config.AddServerGlobalInterceptor<ServerGlobalInterceptor>();

                //config.Option(UraganoOptions.Server_DotNetty_Channel_SoBacklog, 100);
                //config.AddRedisPartitionCaching(new RedisOptions
                //{
                //    ConnectionStrings = new[] { new RedisConnection("192.168.1.254", 6379, "nihao123", false, 15), new RedisConnection("192.168.1.253", 6379, "nihao123", false, 15) }
                //});
                config.AddRedisPartitionCaching<RedisPartitionPolicy>(Configuration.GetSection("Uragano:Caching:Redis"));
                config.Options(Configuration.GetSection("Uragano:Options"));
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
            app.UseUragano();
        }
    }
}
