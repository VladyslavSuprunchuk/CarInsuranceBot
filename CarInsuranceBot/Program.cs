using CarInsuranceBot.Extensions;
using CarInsuranceBot.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarInsuranceBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json")
                               .Build();

            serviceCollection.AddServices()
                             .ConfigureOptions(configuration)
                             .AddLogging(configuration)
                             .AddHttpClients();

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var botService = serviceProvider.GetService<IBotCommunicationService>();

            Task.Run(botService!.StartAsync).Wait();
        }
    }
}