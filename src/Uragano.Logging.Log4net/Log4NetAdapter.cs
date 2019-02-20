using System;
using System.Reflection;
using System.Xml;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Logging;

namespace Uragano.Logging.Log4Net
{
    public class Log4NetAdapter : ILogger
    {
        private readonly ILog Logger;

        public Log4NetAdapter(string name, XmlElement config)
        {
            var repository = LogManager.CreateRepository(
                Assembly.GetEntryAssembly(),
                typeof(log4net.Repository.Hierarchy.Hierarchy)
            );
            XmlConfigurator.Configure(repository, config);
            Logger = LogManager.GetLogger(repository.Name, name);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }
            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message)) return;
            switch (logLevel)
            {
                case LogLevel.Critical:
                    Logger.Fatal(message, exception);
                    break;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    Logger.Debug(message, exception);
                    break;
                case LogLevel.Error:
                    Logger.Error(message, exception);
                    break;
                case LogLevel.Information:
                    Logger.Info(message, exception);
                    break;
                case LogLevel.Warning:
                    Logger.Warn(message, exception);
                    break;
                default:
                    Logger.Info(message, exception);
                    break;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return Logger.IsFatalEnabled;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return Logger.IsDebugEnabled;
                case LogLevel.Error:
                    return Logger.IsErrorEnabled;
                case LogLevel.Information:
                    return Logger.IsInfoEnabled;
                case LogLevel.Warning:
                    return Logger.IsWarnEnabled;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
