using AkiraserverV4.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SampleServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            ServiceProvider serviceProvider = ConfigureServices();

            Listener serv = new Listener(serviceProvider);

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
            return services.BuildServiceProvider();
        }

        private static IConfiguration LoadConfiguration(IServiceProvider arg)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder = builder.AddJsonFile("config.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }
}