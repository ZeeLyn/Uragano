using System.Reflection;

namespace Uragano.Abstractions
{
    public interface ICachingKeyGenerator
    {
        string Generate(string route, MethodInfo methodInfo, object[] args);
    }
}
