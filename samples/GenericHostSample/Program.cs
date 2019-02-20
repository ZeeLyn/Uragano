using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uragano.Consul;
using Uragano.Core;

namespace GenericHostSample
{
    class Program
    {

        static async Task Main(string[] args)
        {

            var hostBuilder = new HostBuilder().ConfigureHostConfiguration(builder =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    //builder.AddJsonFile("appsettings.json", true, true);
                    builder.AddJsonFile("uragano.json", true, true);
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentVariables("uragano");
                }).ConfigureAppConfiguration((context, builder) =>
                {

                })
                .ConfigureServices((context, service) =>
                {
                    service.AddUragano(context.Configuration, builder =>
                    {
                        builder.AddServer();
                        builder.AddClient();
                        builder.AddConsul();
                    });
                }).ConfigureLogging((context, builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                });
            await hostBuilder.RunConsoleAsync();
        }
    }
}
