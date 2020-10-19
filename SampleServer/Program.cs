using AkiraserverV4.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SampleServer
{
    // Benchmark Command
    // gobench -u http://localhost:80/Count -c 10000 -k=true  -t 10
    internal static class Program
    {
        public static readonly IServiceProvider ServiceProvider = ConfigureServices();
        public static async Task Main()
        {
            AkiraServerV4 serv = new AkiraServerV4(ServiceProvider);
            serv.LoadRouting(Assembly.GetExecutingAssembly());
            //serv.SetMiddleware<Middleware.Middleware>();

            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                serv.StopListening();
            };

            await serv.StartListening().ConfigureAwait(false);
        }

        private static IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton(LoadConfiguration);
            services.AddSingleton<ISampleService, SampleService>();
            services.AddScoped(LoggerFactoryConf);
            services.AddLogging();
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
                    //.AddEventLog()
                    ;
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