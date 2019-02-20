using System.Collections.Concurrent;
using System.IO;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace Uragano.Logging.Log4Net
{
    public class Log4NetProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Log4NetAdapter> _loggers =
            new ConcurrentDictionary<string, Log4NetAdapter>();

        private static XmlElement XmlElement { get; set; }

        private string ConfigXmlFile { get; }

        public Log4NetProvider(string configFilename)
        {
            ConfigXmlFile = configFilename;
        }

        public void Dispose()
        {
            _loggers.Clear();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, new Log4NetAdapter(categoryName, ReadConfigFile(ConfigXmlFile)));
        }
        private static XmlElement ReadConfigFile(string filename)
        {
            if (XmlElement != null)
                return XmlElement;
            var log4NetConfig = new XmlDocument();
            using (var stream = File.OpenRead(filename))
            {
                log4NetConfig.Load(stream);
                return XmlElement = log4NetConfig["log4net"];
            }
        }
    }
}
