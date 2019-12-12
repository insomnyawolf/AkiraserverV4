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
            var config = LoadConfiguration();

            var serverConfig = config.GetSection("Server");

            Listener<SampleContext> serv = new Listener<SampleContext>(serverConfig);


            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                serv.StopListening();
            };

            await serv.StartListening();
        }

        private static IConfiguration LoadConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder = builder.AddJsonFile("config.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }

        private static ServiceProvider ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();
            return services.BuildServiceProvider();
        }
    }
}
