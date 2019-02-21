using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Uragano.DynamicProxy
{
    public class ReflectHelper
    {
        private static readonly object LockObject = new object();

        private static List<Type> Types { get; set; }

        private ReflectHelper()
        {
        }

        public static List<Type> GetDependencyTypes()
        {
            lock (LockObject)
            {
                if (Types != null)
                    return Types;
                var ignoreAssemblyFix = new[]
                {
                    "Microsoft", "System", "Consul", "Polly", "Newtonsoft.Json", "MessagePack", "Google.Protobuf","DotNetty","Exceptionless","CSRedis","SafeObjectPool",
                    "Remotion.Linq", "SOS.NETCore", "WindowsBase", "mscorlib", "netstandard", "Uragano.Abstractions","Uragano.Core","Uragano.DynamicProxy","Uragano.Logging.Exceptionless"
                };

                var assemblies = DependencyContext.Default.RuntimeLibraries.SelectMany(i =>
                    i.GetDefaultAssemblyNames(DependencyContext.Default)
                        .Where(p => !ignoreAssemblyFix.Any(ignore =>
                            p.Name.StartsWith(ignore, StringComparison.CurrentCultureIgnoreCase)))
                        .Select(z => Assembly.Load(new AssemblyName(z.Name)))).Where(p => !p.IsDynamic).ToList();

                Types = assemblies.SelectMany(p => p.GetExportedTypes()).ToList();
                return Types;
            }
        }

        public static Type Find(string typeFullName)
        {
            return Types.FirstOrDefault(p => p.FullName == typeFullName);
        }
    }
}
