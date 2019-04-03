using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Logging;
using Uragano.Abstractions;
using Uragano.Abstractions.ServiceDiscovery;

namespace Uragano.Consul
{
    public class ConsulServiceDiscovery : IServiceDiscovery
    {
        private ILogger Logger { get; }
        private UraganoSettings UraganoSettings { get; }

        private static readonly ConcurrentDictionary<string, List<ServiceNodeInfo>> ServiceNodes =
            new ConcurrentDictionary<string, List<ServiceNodeInfo>>();

        public event NodeLeaveHandler OnNodeLeave;
        public event NodeJoinHandler OnNodeJoin;
        public ConsulServiceDiscovery(UraganoSettings uraganoSettings, ILogger<ConsulServiceDiscovery> logger)
        {
            UraganoSettings = uraganoSettings;
            Logger = logger;
        }

        public async Task<bool> RegisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, IServiceRegisterConfiguration serviceRegisterConfiguration, int? weight = default, CancellationToken cancellationToken = default)
        {
            if (!(serviceDiscoveryClientConfiguration is ConsulClientConfigure client))
                return false;

            if (serviceRegisterConfiguration == null)
            {
                serviceRegisterConfiguration = new ConsulRegisterServiceConfiguration();
            }

            if (!(serviceRegisterConfiguration is ConsulRegisterServiceConfiguration service))
                throw new ArgumentNullException(nameof(UraganoSettings.ServiceRegisterConfiguration));

            if (string.IsNullOrWhiteSpace(serviceRegisterConfiguration.Name))
                throw new ArgumentNullException(nameof(serviceRegisterConfiguration.Name), "Service name value cannot be null.");

            Logger.LogTrace("Start registering with consul[{0}]...", client.Address);
            try
            {
                using (var consul = new ConsulClient(conf =>
                {
                    conf.Address = client.Address;
                    conf.Datacenter = client.Datacenter;
                    conf.Token = client.Token;
                    conf.WaitTime = client.WaitTime;
                }))
                {
                    if (weight.HasValue)
                    {
                        if (service.Meta == null)
                            service.Meta = new Dictionary<string, string>();
                        service.Meta.Add("X-Weight", weight.ToString());
                    }

                    //Register service to consul agent 
                    var result = await consul.Agent.ServiceRegister(new AgentServiceRegistration
                    {
                        Address = UraganoSettings.ServerSettings.Address,
                        Port = UraganoSettings.ServerSettings.Port,
                        ID = service.Id,
                        Name = service.Name,
                        EnableTagOverride = service.EnableTagOverride,
                        Meta = service.Meta,
                        Tags = service.Tags,
                        Check = new AgentServiceCheck
                        {
                            TCP = UraganoSettings.ServerSettings.ToString(),
                            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(20),
                            Timeout = TimeSpan.FromSeconds(3),
                            Interval = service.HealthCheckInterval
                        }
                    }, cancellationToken);
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        Logger.LogError("Registration service failed:{0}", result.StatusCode);
                        throw new ConsulRequestException("Registration service failed.", result.StatusCode);
                    }

                    Logger.LogTrace("Consul service registration completed");
                    return result.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Registration service failed:{0}", e.Message);
                return false;
            }
        }

        public async Task<bool> DeregisterAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName, string serviceId)
        {
            if (!(serviceDiscoveryClientConfiguration is ConsulClientConfigure client))
                throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
            if (string.IsNullOrWhiteSpace(serviceId))
                throw new ArgumentNullException(nameof(serviceId));
            Logger.LogTrace("Start deregistration consul...");
            using (var consul = new ConsulClient(conf =>
            {
                conf.Address = client.Address;
                conf.Datacenter = client.Datacenter;
                conf.Token = client.Token;
                conf.WaitTime = client.WaitTime;
            }))
            {
                try
                {
                    var result = await consul.Agent.ServiceDeregister(serviceId);
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        Logger.LogError("Deregistration service failed:{0}", result.StatusCode);
                        throw new ConsulRequestException("Deregistration service failed.", result.StatusCode);
                    }

                    Logger.LogTrace("Deregistration consul has been completed.");
                    return result.StatusCode == HttpStatusCode.OK;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, e.Message);
                    return false;
                }
            }
        }

        public async Task<IReadOnlyList<ServiceDiscoveryInfo>> QueryServiceAsync(IServiceDiscoveryClientConfiguration serviceDiscoveryClientConfiguration, string serviceName, CancellationToken cancellationToken = default)
        {
            if (!(serviceDiscoveryClientConfiguration is ConsulClientConfigure client))
                throw new ArgumentNullException(nameof(serviceDiscoveryClientConfiguration));
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentNullException(nameof(serviceName));
            using (var consul = new ConsulClient(conf =>
            {
                conf.Address = client.Address;
                conf.Datacenter = client.Datacenter;
                conf.Token = client.Token;
                conf.WaitTime = client.WaitTime;
            }))
            {
                try
                {
                    var result = await consul.Health.Service(serviceName, "", true, cancellationToken);

                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        Logger.LogError("Query the service {0} failed:{0}", serviceName, result.StatusCode);
                        return new List<ServiceDiscoveryInfo>();
                    }

                    if (!result.Response.Any())
                        return new List<ServiceDiscoveryInfo>();

                    return result.Response.Select(p => new ServiceDiscoveryInfo(p.Service.ID, p.Service.Address, p.Service.Port, p.Service.Meta)).ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Query the service {2} error:{0}\n{1}", ex.Message, ex.StackTrace, serviceName);
                    throw;
                }
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyList<ServiceNodeInfo>> GetAllService()
        {
            return ServiceNodes.ToDictionary(k => k.Key, v => (IReadOnlyList<ServiceNodeInfo>)v.Value);
        }

        public async Task<IReadOnlyList<ServiceNodeInfo>> GetServiceNodes(string serviceName)
        {
            if (ServiceNodes.TryGetValue(serviceName, out var result))
                return result;
            var serviceNodes = await QueryServiceAsync(UraganoSettings.ServiceDiscoveryClientConfiguration, serviceName);
            if (!serviceNodes.Any())
            {
                return new List<ServiceNodeInfo>();
            }
            var nodes = serviceNodes.Select(p => new ServiceNodeInfo(p.ServiceId, p.Address, p.Port, int.Parse(p.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0"), p.Meta)).ToList();

            if (ServiceNodes.TryAdd(serviceName, nodes))
                return nodes;

            throw new InvalidOperationException($"Service {serviceName} not found.");
        }

        public void AddNode(string serviceName, params ServiceNodeInfo[] nodeInfo)
        {
            if (ServiceNodes.TryGetValue(serviceName, out var services))
                services.AddRange(nodeInfo);
            else
                ServiceNodes.TryAdd(serviceName, nodeInfo.ToList());

            OnNodeJoin?.Invoke(serviceName, nodeInfo);
        }

        public void RemoveNode(string serviceName, params string[] servicesId)
        {
            if (!ServiceNodes.TryGetValue(serviceName, out var services)) return;
            services.RemoveAll(p => servicesId.Any(n => n == p.ServiceId));
            OnNodeLeave?.Invoke(serviceName, servicesId);
        }
    }
}
