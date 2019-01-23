using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;


namespace Uragano.DynamicProxy
{
	internal static class CompilationUtilitys
	{
		public static MemoryStream CompileClientProxy(IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references, ILogger logger = null)
		{

			var assemblys = new[]
			{
				"System.Runtime",
				"mscorlib",
				"System.Threading.Tasks",
				"System.Collections"
			};
			references = assemblys.Select(i => MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName(i)).Location)).Concat(references);

			references = references.Concat(new[]
			{
				MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location),
				MetadataReference.CreateFromFile(typeof(DynamicProxyAbstract).GetTypeInfo().Assembly.Location)
			});
			return Compile("Uragano.DynamicProxy.UraganoProxy", trees, references, logger);
		}


		public static MemoryStream Compile(string assemblyName, IEnumerable<SyntaxTree> trees, IEnumerable<MetadataReference> references, ILogger logger = null)
		{
			var compilation = CSharpCompilation.Create(assemblyName, trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			var stream = new MemoryStream();
			var result = compilation.Emit(stream);
			if (!result.Success && logger != null)
			{
				foreach (var message in result.Diagnostics.Select(i => i.ToString()))
				{
					logger.LogError(message);
				}
				return null;
			}
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}
	}
}