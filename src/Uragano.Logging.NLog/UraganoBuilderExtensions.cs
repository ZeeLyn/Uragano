using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;
using Uragano.Abstractions;


namespace Uragano.Logging.NLog
{
    public static class UraganoBuilderExtensions
    {
        public static void AddNLogLogger(this IUraganoBuilder builder, string configXmlFile = "nlog.config")
        {
            LogManager.LoadConfiguration(Path.Combine(Directory.GetCurrentDirectory(), configXmlFile));
            builder.AddLogger(new NLogLoggerProvider());
        }
    }
}
