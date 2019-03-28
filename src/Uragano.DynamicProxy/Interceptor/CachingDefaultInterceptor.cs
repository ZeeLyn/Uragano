using System;
using System.Threading.Tasks;
using Uragano.Abstractions;
using Uragano.Abstractions.Service;

namespace Uragano.DynamicProxy.Interceptor
{
    public sealed class CachingDefaultInterceptor : InterceptorAbstract
    {
        private ICaching Caching { get; }
        private ICachingKeyGenerator KeyGenerator { get; }
        private ICachingOptions CachingOptions { get; }
        private IServiceFactory ServiceFactory { get; }



        public CachingDefaultInterceptor(ICaching caching, ICachingKeyGenerator keyGenerator, UraganoSettings uraganoSettings, IServiceFactory serviceFactory)
        {
            Caching = caching;
            KeyGenerator = keyGenerator;
            CachingOptions = uraganoSettings.CachingOptions;
            ServiceFactory = serviceFactory;
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            var service = ServiceFactory.Get(context.ServiceRoute);
            if (service.CachingConfig == null) return await context.Next();
            var key = KeyGenerator.ReplacePlaceholder(service.CachingConfig.KeyPlaceholder, service.CachingConfig.CustomKey,
                context.Args);
            if (!(context is InterceptorContext ctx))
                throw new ArgumentNullException();
            var (value, hasKey) = await Caching.Get(key, ctx.ReturnType);
            if (hasKey)
                return new ServiceResult(value);
            var result = await context.Next();
            if (result.Status != RemotingStatus.Ok)
                return result;
            await Caching.Set(key, result.Result, service.CachingConfig.ExpireSeconds);
            return result;
        }
    }
}
