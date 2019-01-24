using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Uragano.DynamicProxy
{
	public class ReflectHelper
	{
		private static readonly object lockObject = new object();

		private static List<Type> types { get; set; }

		private ReflectHelper()
		{
		}

		public static List<Type> GetDependencyTypes()
		{
			lock (lockObject)
			{
				if (types != null)
					return types;
				var ignoreAssemblyFix = new[]
				{
					"Microsoft", "System", "Consul", "Polly", "Newtonsoft.Json", "MessagePack", "Google.Protobuf",
					"Remotion.Linq", "SOS.NETCore", "WindowsBase", "mscorlib", "netstandard", "Uragano"
				};

				var assemblies = DependencyContext.Default.RuntimeLibraries.SelectMany(i =>
					i.GetDefaultAssemblyNames(DependencyContext.Default)
						.Where(p => !ignoreAssemblyFix.Any(ignore =>
							p.Name.StartsWith(ignore, StringComparison.CurrentCultureIgnoreCase)))
						.Select(z => Assembly.Load(new AssemblyName(z.Name)))).Where(p => !p.IsDynamic).ToList();

				types = assemblies.SelectMany(p => p.GetExportedTypes()).ToList();
				return types;
			}
		}
	}
}
