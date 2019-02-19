using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Uragano.Consul;
using Uragano.Core;

namespace GenericHostSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            var host = hostBuilder.ConfigureHostConfiguration(config =>
                {
                    //config.SetBasePath(Directory.GetCurrentDirectory());
                }).
                ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("uragano.json");
                }).ConfigureServices((context, service) =>
                {
                    service.AddUragano(context.Configuration, builder =>
                    {
                        builder.AddServer();
                        builder.AddConsul();
                    });
                }).ConfigureLogging(config => { }).UseConsoleLifetime().Build();
            await host.RunAsync();
        }
    }
}
