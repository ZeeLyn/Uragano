using Uragano.Abstractions;

namespace Uragano.Logging.Log4Net
{
    public static class UraganoBuilderExtensions
    {
        public static void AddLog4NetLogger(this IUraganoBuilder builder, string configXmlFile = "log4net.config")
        {
            builder.AddLogger(new Log4NetProvider(configXmlFile));
        }
    }
}
