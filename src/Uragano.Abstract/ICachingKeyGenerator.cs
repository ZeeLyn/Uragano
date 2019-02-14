using System.Reflection;

namespace Uragano.Abstractions
{
    public interface ICachingKeyGenerator
    {
        CachingConfig GenerateKeyPlaceholder(ICachingOptions cachingOptions, string route, MethodInfo methodInfo);

        string ReplacePlaceholder(string keyPlaceholder, bool customKey, object[] args);
    }
}
