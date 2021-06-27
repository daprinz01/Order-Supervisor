using Agent_App.HostedServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using StorageBroker.Implementation;
using StorageBroker.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Agent_App
{
    class Program
    {

        private const string _appsettings = "appsettings.json";
        private const string _hostings = "hostingsettings.json";
        static Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().Enrich
            .FromLogContext()
            .WriteTo.File($"Logs/Agent-app.log", Serilog.Events.LogEventLevel.Debug, rollOnFileSizeLimit: true, fileSizeLimitBytes: 10_000_000)
            .WriteTo.Console(Serilog.Events.LogEventLevel.Debug)
            .CreateLogger();
            using IHost host = CreateHostBuilder(args).Build();
            return host.RunAsync();
        }
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .UseConsoleLifetime()
            .ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile(_hostings, optional: true);
                configHost.AddCommandLine(args);
            })
            .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile(_appsettings, optional: true);
                    configApp.AddJsonFile(
                        $"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                    configApp.AddCommandLine(args);
                })
            .ConfigureServices((_, services) =>
                {
                    services.AddSingleton<IQueueBroker, QueueBroker>();
                    services.AddSingleton<ITableBroker, TableBroker>();
                    services.AddTransient<IUtilities, Utilities>();
                    services.AddHostedService<OrderProcessorManagerService>();
                });

    }
}
