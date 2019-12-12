using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SuperSimpleHttpListener
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var config = LoadConfiguration();

            var serverConfig = config.GetSection("Server");

            Http.Listener serv = new Http.Listener(serverConfig);

            serv.StartListening();

            await Task.Delay(-1);
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