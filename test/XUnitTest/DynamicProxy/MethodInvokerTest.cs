using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uragano.DynamicProxy;
using Xunit;

namespace XUnitTest.DynamicProxy
{
    public class MethodInvokerTest
    {


        [Fact]
        public void ConstructMethodTest()
        {
            var method = typeof(MethodInvokerMethod).GetMethod("GetString");
            Assert.Throws<ArgumentNullException>(() =>
            {
                new MethodInvoker(null);
            });
            var methodInvoker = new MethodInvoker(method);
            Assert.NotNull(methodInvoker);
        }

        [Fact]
        public void InvokerAsyncTest()
        {
            var type = typeof(MethodInvokerMethod);
            var method1 = type.GetMethod("GetString");
            var methodInvoker1 = new MethodInvoker(method1);
            var result1 = methodInvoker1.InvokeAsync(new MethodInvokerMethod()).GetAwaiter().GetResult();
            Assert.Equal("test string", result1);

            var method2 = type.GetMethod("Exec");
            var methodInvoker2 = new MethodInvoker(method2);
            var result2 = methodInvoker2.InvokeAsync(new MethodInvokerMethod()).GetAwaiter().GetResult();
            Assert.Null(result2);
        }
    }

    public class MethodInvokerMethod
    {
        public async Task<string> GetString()
        {
            return await Task.FromResult("test string");
        }

        public async Task Exec()
        {
            await Task.CompletedTask;
        }
    }
}
