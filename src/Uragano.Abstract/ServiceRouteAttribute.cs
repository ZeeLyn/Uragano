using System;
using System.Text.RegularExpressions;

namespace Uragano.Abstractions
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class ServiceRouteAttribute : Attribute
    {
        public string Route { get; set; }

        public ServiceRouteAttribute(string route)
        {
            if (!Regex.IsMatch(route, "^[a-zA-Z0-9/_-]+$"))
                throw new ArgumentException("路由只允许包含字母，数字，下划线，减号，斜杠");
            Route = route;
        }
    }
}
