using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Uragano.Abstractions.CircuitBreaker
{
    public interface IScriptInjection
    {
        bool AddScript(string route, string script, string[] usingNamespaces = default);

        ScriptDescriptor GetScript(string route);

        Task<object> Run(string route);
    }
}
