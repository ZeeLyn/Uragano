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
using Microsoft.Extensions.DependencyInjection;


namespace Uragano.Consul
{
    public class ConsulServiceDiscovery : IServiceDiscovery
    {
        private ILogger Logger { get; }
        private ServerSettings ServerSettings { get; }

        private ConsulClientConfigure ConsulClientConfigure { get; }

        private ConsulRegisterServiceConfiguration ConsulRegisterServiceConfiguration { get; set; }

        private static readonly AsyncLock AsyncLock = new AsyncLock();

        private static readonly ConcurrentDictionary<string, List<ServiceNodeInfo>> ServiceNodes = new ConcurrentDictionary<string, List<ServiceNodeInfo>>();

        public event NodeLeaveHandler OnNodeLeave;
        public event NodeJoinHandler OnNodeJoin;
        public ConsulServiceDiscovery(UraganoSettings uraganoSettings, ILogger<ConsulServiceDiscovery> logger, IServiceDiscoveryClientConfiguration clientConfiguration, IServiceProvider service)
        {
            if (!(clientConfiguration is ConsulClientConfigure client))
                throw new ArgumentNullException(nameof(clientConfiguration));
            var agent = service.GetService<IServiceRegisterConfiguration>();
            if (agent != null)
            {
                if (!(agent is ConsulRegisterServiceConfiguration serviceAgent))
                    throw new ArgumentNullException(nameof(ConsulRegisterServiceConfiguration));
                ConsulRegisterServiceConfiguration = serviceAgent;
            }


            ConsulClientConfigure = client;
            ServerSettings = uraganoSettings.ServerSettings;
            Logger = logger;
        }

        public async Task<bool> RegisterAsync(CancellationToken cancellationToken = default)
        {
            if (ConsulRegisterServiceConfiguration == null)
            {
                ConsulRegisterServiceConfiguration = new ConsulRegisterServiceConfiguration();
            }

            if (string.IsNullOrWhiteSpace(ConsulRegisterServiceConfiguration.Id))
            {
                ConsulRegisterServiceConfiguration.Id = ServerSettings.ToString();
            }

            if (string.IsNullOrWhiteSpace(ConsulRegisterServiceConfiguration.Name))
            {
                ConsulRegisterServiceConfiguration.Name = ServerSettings.ToString();
            }

            if (string.IsNullOrWhiteSpace(ConsulRegisterServiceConfiguration.Name))
                throw new ArgumentNullException(nameof(ConsulRegisterServiceConfiguration.Name), "Service name value cannot be null.");

            Logger.LogTrace("Start registering with consul[{0}]...", ConsulClientConfigure.Address);

            try
            {
                using (var consul = new ConsulClient(conf =>
                {
                    conf.Address = ConsulClientConfigure.Address;
                    conf.Datacenter = ConsulClientConfigure.Datacenter;
                    conf.Token = ConsulClientConfigure.Token;
                    conf.WaitTime = ConsulClientConfigure.WaitTime;
                }))
                {
                    if (ServerSettings.Weight.HasValue)
                    {
                        if (ConsulRegisterServiceConfiguration.Meta == null)
                            ConsulRegisterServiceConfiguration.Meta = new Dictionary<string, string>();
                        ConsulRegisterServiceConfiguration.Meta.Add("X-Weight", ServerSettings.Weight.ToString());
                    }

                    //Register service to consul agent 
                    var result = await consul.Agent.ServiceRegister(new AgentServiceRegistration
                    {
                        Address = ServerSettings.Address,
                        Port = ServerSettings.Port,
                        ID = ConsulRegisterServiceConfiguration.Id,
                        Name = ConsulRegisterServiceConfiguration.Name,
                        EnableTagOverride = ConsulRegisterServiceConfiguration.EnableTagOverride,
                        Meta = ConsulRegisterServiceConfiguration.Meta,
                        Tags = ConsulRegisterServiceConfiguration.Tags,
                        Check = new AgentServiceCheck
                        {
                            TCP = ServerSettings.ToString(),
                            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(20),
                            Timeout = TimeSpan.FromSeconds(3),
                            Interval = ConsulRegisterServiceConfiguration.HealthCheckInterval
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

        public async Task<bool> DeregisterAsync()
        {
            Logger.LogTrace("Start deregistration consul...");
            using (var consul = new ConsulClient(conf =>
            {
                conf.Address = ConsulClientConfigure.Address;
                conf.Datacenter = ConsulClientConfigure.Datacenter;
                conf.Token = ConsulClientConfigure.Token;
                conf.WaitTime = ConsulClientConfigure.WaitTime;
            }))
            {
                try
                {
                    var result = await consul.Agent.ServiceDeregister(ConsulRegisterServiceConfiguration.Id);
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

        public async Task<IReadOnlyList<ServiceDiscoveryInfo>> QueryServiceAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentNullException(nameof(serviceName));
            using (var consul = new ConsulClient(conf =>
            {
                conf.Address = ConsulClientConfigure.Address;
                conf.Datacenter = ConsulClientConfigure.Datacenter;
                conf.Token = ConsulClientConfigure.Token;
                conf.WaitTime = ConsulClientConfigure.WaitTime;
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

                    return result.Response.Select(p => new ServiceDiscoveryInfo(p.Service.ID, p.Service.Address, p.Service.Port, int.Parse(p.Service.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0"), p.Service.Meta)).ToList();
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
            var serviceNodes = await QueryServiceAsync(serviceName);
            if (!serviceNodes.Any())
            {
                return new List<ServiceNodeInfo>();
            }
            var nodes = serviceNodes.Select(p => new ServiceNodeInfo(p.ServiceId, p.Address, p.Port, p.Weight, p.Meta)).ToList();

            if (ServiceNodes.TryAdd(serviceName, nodes))
                return nodes;

            throw new InvalidOperationException($"Service {serviceName} not found.");
        }

        public async Task NodeMonitor(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Start refresh service status,waiting for locking...");
            using (await AsyncLock.LockAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                foreach (var service in ServiceNodes)
                {
                    Logger.LogTrace($"Service {service.Key} refreshing...");
                    var healthNodes = await QueryServiceAsync(service.Key, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var leavedNodes = service.Value.Where(p => healthNodes.All(a => a.ServiceId != p.ServiceId)).Select(p => p.ServiceId).ToArray();
                    if (leavedNodes.Any())
                    {
                        //RemoveNode(service.Key, leavedNodes);
                        if (!ServiceNodes.TryGetValue(service.Key, out var services)) return;
                        services.RemoveAll(p => leavedNodes.Any(n => n == p.ServiceId));
                        OnNodeLeave?.Invoke(service.Key, leavedNodes);
                        Logger.LogTrace($"These nodes are gone:{string.Join(",", leavedNodes)}");
                    }

                    var addedNodes = healthNodes.Where(p =>
                        service.Value.All(e => e.ServiceId != p.ServiceId)).Select(p => new ServiceNodeInfo(p.ServiceId, p.Address, p.Port, int.Parse(p.Meta?.FirstOrDefault(m => m.Key == "X-Weight").Value ?? "0"), p.Meta)).ToList();

                    if (addedNodes.Any())
                    {
                        //AddNode(service.Key, addedNodes);
                        if (ServiceNodes.TryGetValue(service.Key, out var services))
                            services.AddRange(addedNodes);
                        else
                            ServiceNodes.TryAdd(service.Key, addedNodes);

                        OnNodeJoin?.Invoke(service.Key, addedNodes);

                        Logger.LogTrace($"New nodes added:{string.Join(",", addedNodes.Select(p => p.Address + ":" + p.Port))}");
                    }
                }
                Logger.LogTrace("Complete refresh.");
            }
        }
    }
}
