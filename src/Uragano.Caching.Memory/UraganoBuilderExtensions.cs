using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Uragano.Abstractions;

namespace Uragano.Caching.Memory
{
    public static class UraganoBuilderExtensions
    {
        public static void AddMemoryCaching(this IUraganoBuilder builder)
        {
            var config = new MemoryCachingOptions();
            builder.AddMemoryCaching(config);
        }

        public static void AddMemoryCaching(this IUraganoBuilder builder, MemoryCachingOptions memoryCachingOptions)
        {
            builder.AddCaching<MemoryCaching>(memoryCachingOptions);
            builder.ServiceCollection.AddSingleton<IMemoryCache>(new MemoryCache(Options.Create(memoryCachingOptions)));
        }

        public static void AddMemoryCaching(this IUraganoBuilder builder, IConfigurationSection configurationSection)
        {
            var config = configurationSection.Get<MemoryCachingOptions>();
            builder.AddMemoryCaching(config);
        }

        public static void AddMemoryCaching(this IUraganoSampleBuilder builder)
        {
            var config = builder.Configuration.GetSection("Uragano:Caching:Memory").Get<MemoryCachingOptions>();
            builder.AddMemoryCaching(config);
        }
    }
}
