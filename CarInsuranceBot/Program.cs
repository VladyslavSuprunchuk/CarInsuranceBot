using CarInsuranceBot.Core.Const;
using CarInsuranceBot.Core.Options;
using CarInsuranceBot.Services.Interfaces;
using CarInsuranceBot.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

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

            //services initialization
            serviceCollection.AddScoped<IBotCommunicationService, BotCommunicationService>();
            serviceCollection.AddScoped<IParserService, ParserService>();
            serviceCollection.AddScoped<IFileService, FileService>();
            serviceCollection.AddScoped<IOpenaiService, OpenaiService>();

            //options binding
            serviceCollection.AddOptions<MindeeOptions>().Bind(configuration.GetSection(MindeeOptions.Mindee));
            serviceCollection.AddOptions<TelegramBotOptions>().Bind(configuration.GetSection(TelegramBotOptions.TelegramBot));
            serviceCollection.AddOptions<OpenaiOptions>().Bind(configuration.GetSection(OpenaiOptions.Openai));

            //httpClient initialization
            serviceCollection.AddHttpClient(HttpClientKeywords.ClientTitle, client =>
            {
                client.BaseAddress = new Uri(HttpClientKeywords.TelegramBaseUrl);
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var botService = serviceProvider.GetService<IBotCommunicationService>();

            Task.Run(botService!.StartAsync).Wait();
        }
    }
}