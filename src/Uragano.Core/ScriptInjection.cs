using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Uragano.Abstractions.CircuitBreaker;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Uragano.Core
{
    public class ScriptInjection : IScriptInjection
    {
        private static readonly ConcurrentDictionary<string, ScriptDescriptor> ServiceCommand =
            new ConcurrentDictionary<string, ScriptDescriptor>();

        private static readonly ConcurrentDictionary<string, object> ScriptResult = new ConcurrentDictionary<string, object>();

        public bool AddScript(string route, string script, string[] usingNamespaces = default)
        {
            return ServiceCommand.TryAdd(route, new ScriptDescriptor
            {
                Script = script,
                UsingNamespaces = usingNamespaces
            });
        }

        public ScriptDescriptor GetScript(string route)
        {
            if (ServiceCommand.TryGetValue(route, out var value))
                return value;
            throw new InvalidOperationException($"No script found for route {route}");
        }

        public async Task<object> Run(string route)
        {
            if (ScriptResult.TryGetValue(route, out var value))
                return value;

            var script = GetScript(route);
            var scriptOptions = ScriptOptions.Default.WithImports("System.Threading.Tasks");
            if (script.UsingNamespaces != null && script.UsingNamespaces.Length > 0)
            {
                scriptOptions = scriptOptions.WithReferences(script.UsingNamespaces);
                scriptOptions = scriptOptions.AddImports(script.UsingNamespaces);
            }
            var result = await CSharpScript.EvaluateAsync(script.Script, scriptOptions);
            ScriptResult.TryAdd(route, result);
            return result;
        }
    }
}
