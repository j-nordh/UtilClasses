using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using UtilClasses.Extensions.Strings;
using UtilClasses.Json;

namespace UtilClasses.Services
{
    public static class Helper
    {
        public static void SetupDefaultConfig(WebApplicationBuilder builder, JsonSerializerSettings? settings = null)
        {
            if (null != settings)
            {
                JsonConvert.DefaultSettings = () => settings;
                builder.Services.AddMvc().AddNewtonsoftJson(s => JsonUtil.ApplySettings(s.SerializerSettings));
            }

            //let's use AppSettings, allow for DEBUG, RELEASE and Production versions and make environment variables accessible.
            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
                .AddJsonFile("appsettings.Development.json", optional: true)
#else
                .AddJsonFile("appsettings.Production.json", optional: true)
#endif
                .AddEnvironmentVariables();
        }

        public static void SetupLogging(WebApplicationBuilder builder, string logDir,
            Action<LoggerConfiguration>? setup = null)
        {
            var cfg = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console();
            if (logDir.IsNotNullOrEmpty())
                cfg.WriteTo.File(logDir ?? "", fileSizeLimitBytes: 0xA00000L, rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 20);
            setup?.Invoke(cfg);

            Log.Logger = cfg.CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);
        }

        private class SkipStatusRequest : ILogEventFilter
        {
            private string? GetValue(LogEvent logEvent, string key) => logEvent.Properties.TryGetValue(key, out var val)
                ? val.ToString()
                : null;

            public bool IsEnabled(LogEvent logEvent)
            {
                if (logEvent.Properties.Count == 0)
                    return true;
                if (logEvent.MessageTemplate.Text.ContainsOic("MQTT")) return true;
                var uri = GetValue(logEvent, "Uri");
                var path = GetValue(logEvent, "Path");
                var requestPath = GetValue(logEvent, "RequestPath");
                var context = GetValue(logEvent, "SourceContext");
                var ping = "/ping";
                if (uri.ContainsOic(ping) || path.ContainsOic(ping) || requestPath.ContainsOic(ping)) return false;


                if (context.ContainsOic("HttpClient"))
                {
                    var eventId = GetValue(logEvent, "EventId");
                    return eventId.ContainsOic("RequestEnd");
                }

                return !logEvent.MessageTemplate.Text.Contains("HTTP");
            }
        }
    }
}