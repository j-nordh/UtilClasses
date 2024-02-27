using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MACS.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using UtilClasses.Extensions.Strings;

namespace MACS.Service
{
    public class Helper
    {
        public static void SetupDefaultConfig(WebApplicationBuilder builder)
        {
            JsonConvert.DefaultSettings = JsonUtil.GetSettings;
            builder.Services.AddMvc().AddNewtonsoftJson(s => JsonUtil.ApplySettings(s.SerializerSettings));
            //let's use AppSettings, allow for DEBUG, RELEASE and Production versions and make environment variables accessible.
            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: true)
                .AddEnvironmentVariables();
        }

        public static void SetupLogging(WebApplicationBuilder builder, string logDir)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Filter.With<SkipStatusRequest>()
                .WriteTo.Console()
                .WriteTo.File(logDir ?? "", fileSizeLimitBytes: 10485760, rollOnFileSizeLimit: true, retainedFileCountLimit: 20)
                .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger);
        }

        private class SkipStatusRequest : ILogEventFilter
        {
            private string GetValue(LogEvent logEvent, string key) => logEvent.Properties.TryGetValue(key, out var val)
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
