using System.Collections.Generic;
using System.Linq;
using Uragano.Abstractions;
using Uragano.DynamicProxy;

namespace Uragano.Core
{
    public static class ServiceExtensions
    {

        public static TService SetMeta<TService>(this TService service, Dictionary<string, string> meta) where TService : IService
        {
            if (service is DynamicProxyAbstract dynamicProxyAbstract)
            {
                foreach (var item in meta)
                {
                    dynamicProxyAbstract.SetMeta(item.Key, item.Value);
                }
            }
            return service;
        }

        public static TService SetMeta<TService>(this TService service, params (string key, string value)[] meta) where TService : IService
        {

            return service.SetMeta(meta.ToDictionary(key => key.key, value => value.value));
        }

        public static TService SetMeta<TService>(this TService service, string key, string value) where TService : IService
        {
            if (service is DynamicProxyAbstract dynamicProxyAbstract)
            {
                dynamicProxyAbstract.SetMeta(key, value);
            }
            return service;
        }
    }
}
