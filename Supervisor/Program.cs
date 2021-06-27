using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using StorageBroker.Implementation;
using StorageBroker.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Supervisor
{
    public class Program
    {
        private const string _appsettings = "appsettings.json";
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File($"Logs/Supervisor.log", Serilog.Events.LogEventLevel.Debug, rollOnFileSizeLimit: true, fileSizeLimitBytes: 10_000_000)
                .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
                .CreateLogger();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
            {
                configApp.SetBasePath(Directory.GetCurrentDirectory());
                configApp.AddJsonFile(_appsettings, optional: true);
                configApp.AddJsonFile(
                    $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                    optional: true);
                configApp.AddCommandLine(args);
            });
    }
}
