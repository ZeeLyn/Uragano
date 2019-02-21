using System.Reflection;

namespace Uragano.Abstractions
{
    public interface ICachingKeyGenerator
    {
        string GenerateKeyPlaceholder(string keyPrefix, int globalExpire, string route, MethodInfo methodInfo, CachingAttribute cachingAttribute = default);

        string ReplacePlaceholder(string keyPlaceholder, bool customKey, object[] args);
    }
}
