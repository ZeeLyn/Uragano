using System.Collections.Generic;
using System.Threading.Tasks;
using Uragano.Abstractions;

namespace Uragano.DynamicProxy
{
    public abstract class DynamicProxyAbstract
    {
        private Dictionary<string, string> Meta { get; set; }

        private IRemotingInvoke RemotingInvoke { get; }

        protected DynamicProxyAbstract(IRemotingInvoke remotingInvoke)
        {
            RemotingInvoke = remotingInvoke;
        }

        public void SetMeta(string key, string value)
        {
            if (Meta == null)
                Meta = new Dictionary<string, string>();
            Meta.Add(key, value);
        }


        //protected void Invoke(object[] args, string route, string serviceName)
        //{
        //    RemotingInvoke.InvokeAsync<object>(args, route, serviceName, Meta).ConfigureAwait(false).GetAwaiter().GetResult();
        //}

        //protected T Invoke<T>(object[] args, string route, string serviceName)
        //{
        //    return RemotingInvoke.InvokeAsync<T>(args, route, serviceName, Meta).ConfigureAwait(false).GetAwaiter().GetResult();
        //}


        protected async Task InvokeAsync(object[] args, string route, string serviceName)
        {
            await RemotingInvoke.InvokeAsync(args, route, serviceName, Meta);
            Meta.Clear();
        }

        protected async Task<T> InvokeAsync<T>(object[] args, string route, string serviceName)
        {
            var result = await RemotingInvoke.InvokeAsync<T>(args, route, serviceName, Meta);
            Meta.Clear();
            return result;
        }
    }
}
