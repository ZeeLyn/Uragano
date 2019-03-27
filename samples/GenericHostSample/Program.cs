using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions.ConsistentHash;
using Uragano.Consul;
using Uragano.Core;
using Uragano.Logging.Exceptionless;

namespace GenericHostSample
{
    class Program
    {
        public class Node
        {
            public Node(string host)
            {
                Host = host;
            }

            public string Host { get; set; }

            public int Num { get; set; }

            public override string ToString()
            {
                return $"{Host}:{Num}";
            }
        }

        static async Task Main(string[] args)
        {

            var nodes = new List<Node> {
                new Node("139f9cbb-be67-460c-9092-b4b29a6e574a"),
                new Node("3f8fa9c8-4c56-486a-baca-bd7d4639e95b"),
                new Node("c2ab77c7-84c7-4310-aaff-9fe767639135"),
                new Node("55cf40c5-250d-4cb8-9cc7-8d1d361a53b0"),
                new Node("6719662f-74b5-46c7-a652-031c3771b812")
            };
            var x = new ConsistentHash<Node>();
            nodes.ForEach(item => { x.AddNode(item, item.Host); });

            while (true)
            {
                var command = Console.ReadLine();
                if (command == "exit")
                    break;
                if (command == "gen")
                {
                    for (var i = 0; i < 100000; i++)
                    {
                        var id = Guid.NewGuid().ToString();
                        var node = x.GetNodeForKey(id);
                        node.Num++;
                    }

                }

                if (command == "info")
                {
                    nodes.ForEach(item =>
                    {
                        Console.WriteLine(item.ToString());
                    });

                }

                if (command == "add")
                {
                    Console.Write("请输入Key:");
                    var k = Console.ReadLine();
                    var node = new Node(k);
                    nodes.Add(node);
                    x.AddNode(node, k);
                }
            }

            //var hostBuilder = new HostBuilder().ConfigureHostConfiguration(builder =>
            //    {
            //        builder.SetBasePath(Directory.GetCurrentDirectory());
            //    }).ConfigureAppConfiguration((context, builder) =>
            //    {
            //        builder.AddJsonFile("uragano.json", false, true);
            //        //builder.AddEnvironmentVariables("uragano");
            //        builder.AddCommandLine(args);
            //    })
            //    .ConfigureServices((context, service) =>
            //    {
            //        service.AddUragano(context.Configuration, builder =>
            //        {
            //            builder.AddServer();
            //            builder.AddClient();
            //            builder.AddCircuitBreaker();
            //            builder.AddExceptionlessLogger();
            //            builder.AddConsul();
            //        });
            //    }).ConfigureLogging((context, builder) =>
            //    {
            //        builder.AddConfiguration(context.Configuration.GetSection("Logging"));
            //        builder.AddConsole();
            //    });
            //await hostBuilder.RunConsoleAsync();
        }
    }
}
