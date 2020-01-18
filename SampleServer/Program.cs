using AkiraserverV4.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SampleServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            ServiceProvider serviceProvider = ConfigureServices();

            AkiraServerV4 serv = new AkiraServerV4(serviceProvider);
            serv.LoadRouting(Assembly.GetExecutingAssembly());

            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                serv.StopListening();
            };

            await serv.StartListening();
        }

        private static ServiceProvider ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton(LoadConfiguration);
            services.AddSingleton<SampleService>();
            services.AddScoped(LoggerFactoryConf);
            return services.BuildServiceProvider();
        }

        private static ILoggerFactory LoggerFactoryConf(IServiceProvider arg)
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFilter("Microsoft", LogLevel.Trace)
                    .AddFilter("System", LogLevel.Trace)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Trace)
                    .AddConsole()
                    /*.AddEventLog()*/;
            });
        }

        private static IConfiguration LoadConfiguration(IServiceProvider arg)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder = builder.AddJsonFile("config.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }
}