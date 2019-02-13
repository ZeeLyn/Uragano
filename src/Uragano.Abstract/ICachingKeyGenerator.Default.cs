using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Uragano.Abstractions
{
    public class CachingKeyGenerator : ICachingKeyGenerator
    {
        public string Generate(string route, MethodInfo methodInfo, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
