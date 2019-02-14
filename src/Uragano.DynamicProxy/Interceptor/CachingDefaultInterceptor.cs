using System;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.DynamicProxy.Interceptor
{
    public class CachingDefaultInterceptor : InterceptorAbstract
    {
        private ICaching Caching { get; }
        private ICachingKeyGenerator KeyGenerator { get; }
        private ICachingOptions CachingOptions { get; }

        public CachingDefaultInterceptor(ICaching caching, ICachingKeyGenerator keyGenerator, UraganoSettings uraganoSettings)
        {
            Caching = caching;
            KeyGenerator = keyGenerator;
            CachingOptions = uraganoSettings.CachingOptions;
        }

        public override async Task<IServiceResult> Intercept(IInterceptorContext context)
        {
            if (!(context is InterceptorContext ctx))
                throw new ArgumentNullException();
            if (!ctx.CachingOption.Enable) return await context.Next();
            var key = KeyGenerator.ReplacePlaceholder(ctx.CachingOption.Key, ctx.CachingOption.CustomKey,
                context.Args);
            var result = await Caching.Get<IServiceResult>(key);
            if (result != null)
                return result;
            result = await context.Next();
            await Caching.Set(key, result, ctx.CachingOption.Expire);
            return result;
        }
    }
}
