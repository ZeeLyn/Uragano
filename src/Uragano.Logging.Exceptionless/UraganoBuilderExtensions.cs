using System;
using Exceptionless;
using Exceptionless.Extensions.Logging;
using Exceptionless.Storage;
using Microsoft.Extensions.Configuration;
using Uragano.Abstractions;

namespace Uragano.Logging.Exceptionless
{
    public static class UraganoBuilderExtensions
    {
        public static void AddExceptionlessLogger(this IUraganoBuilder builder, string apiKey, string serverUrl = "")
        {
            builder.AddLogger(new ExceptionlessLoggerProvider(ExceptionlessClient.Default));

            ExceptionlessClient.Default.Configuration.ApiKey = apiKey;
            if (!string.IsNullOrWhiteSpace(serverUrl))
                ExceptionlessClient.Default.Configuration.ConfigServerUrl = serverUrl;
        }

        public static void AddExceptionlessLogger(this IUraganoBuilder builder, Action<ExceptionlessConfiguration> configure)
        {
            builder.AddLogger(new ExceptionlessLoggerProvider(configure));
        }

        public static void AddExceptionlessLogger(this IUraganoBuilder builder, IConfiguration configuration)
        {
            ExceptionlessClient.Default.Configuration.ReadFromConfiguration(configuration);
            builder.AddLogger(new ExceptionlessLoggerProvider(ExceptionlessClient.Default));
        }
        public static void AddExceptionlessLogger(this IUraganoSampleBuilder builder)
        {
            ExceptionlessClient.Default.Configuration.ReadFromConfiguration(builder.Configuration.GetSection("Uragano:Logging:Exceptionless"));
            builder.AddLogger(new ExceptionlessLoggerProvider(ExceptionlessClient.Default));
        }

        private static void ReadFromConfiguration(this ExceptionlessConfiguration config, IConfiguration settings)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var apiKey = settings["ApiKey"];
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "API_KEY_HERE")
                config.ApiKey = apiKey;

            foreach (var data in settings.GetSection("DefaultData").GetChildren())
                if (data.Value != null)
                    config.DefaultData[data.Key] = data.Value;

            foreach (var tag in settings.GetSection("DefaultTags").GetChildren())
                config.DefaultTags.Add(tag.Value);

            if (bool.TryParse(settings["Enabled"], out var enabled) && !enabled)
                config.Enabled = false;

            if (bool.TryParse(settings["IncludePrivateInformation"], out var includePrivateInformation) && !includePrivateInformation)
                config.IncludePrivateInformation = false;

            var serverUrl = settings["ServerUrl"];
            if (!string.IsNullOrEmpty(serverUrl))
                config.ServerUrl = serverUrl;

            var storagePath = settings["StoragePath"];
            if (!string.IsNullOrEmpty(storagePath))
                config.Resolver.Register(typeof(IObjectStorage), () => new FolderObjectStorage(config.Resolver, storagePath));

            foreach (var setting in settings.GetSection("Settings").GetChildren())
                if (setting.Value != null)
                    config.Settings[setting.Key] = setting.Value;
        }
    }
}
