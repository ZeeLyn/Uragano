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
using Uragano.Logging.Exceptionless;

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
            //services.AddUragano(Configuration);
            services.AddUragano(builder =>
            {
                //builder.IsDevelopment(true);
                builder.AddServer(Configuration.GetSection("Uragano:Server"));
                builder.AddClient();

                builder.AddConsul(Configuration.GetSection("Uragano:ServiceDiscovery:Consul:Client"),
                    Configuration.GetSection("Uragano:ServiceDiscovery:Consul:Service"));
                builder.AddClientGlobalInterceptor<ClientGlobal_1_Interceptor>();
                //builder.AddClientGlobalInterceptor<ClientGlobal_2_Interceptor>();
                //builder.AddServerGlobalInterceptor<ServerGlobalInterceptor>();

                //builder.Option(UraganoOptions.Server_DotNetty_Channel_SoBacklog, 100);
                //builder.AddRedisPartitionCaching(new RedisOptions
                //{
                //    ConnectionStrings = new[] { new RedisConnection("192.168.1.254", 6379, "nihao123", false, 15), new RedisConnection("192.168.1.253", 6379, "nihao123", false, 15) }
                //});
                builder.AddExceptionlessLogger(Configuration.GetSection("Uragano:Logging:Exceptionless"));
                builder.AddRedisPartitionCaching(Configuration.GetSection("Uragano:Caching:Redis"));
                builder.Options(Configuration.GetSection("Uragano:Options"));
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
