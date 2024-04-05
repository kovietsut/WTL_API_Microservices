using Common.Logging;
using Serilog;

namespace ElasticSearch.API.Extensions
{
    public static class HostExtensions
    {
        public static void AddAppConfigurations(this ConfigureHostBuilder host)
        {
            _ = host.ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"errorcode.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
            }).UseSerilog(Serilogger.Configure);
        }
    }
}
