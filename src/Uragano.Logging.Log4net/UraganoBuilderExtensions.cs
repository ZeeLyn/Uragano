using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Logging.Log4Net
{
    public static class UraganoBuilderExtensions
    {
        public static void AddLog4Net(this IUraganoBuilder builder, string configXmlFile)
        {
            builder.AddLogger(new Log4NetProvider(configXmlFile));
        }

        public static void AddLog4Net(this IUraganoBuilder builder, IConfiguration configuration)
        {
            builder.AddLogger(new Log4NetProvider(configuration.GetValue<string>("ConfigXml")));
        }

        public static void AddLog4Net(this IUraganoSampleBuilder builder)
        {
            builder.AddLogger(new Log4NetProvider(builder.Configuration.GetSection("Uragano:Logging:log4net").GetValue<string>("ConfigXml")));
        }
    }
}
