using CarInsuranceBot.Core.Const;
using CarInsuranceBot.Core.Options;
using CarInsuranceBot.Services.Interfaces;
using CarInsuranceBot.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace CarInsuranceBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            //services initialization
            services.AddScoped<IBotCommunicationService, BotCommunicationService>();
            services.AddScoped<IParserService, ParserService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IOpenaiService, OpenaiService>();

            return services;
        }

        public static IServiceCollection ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
        {
            //options binding
            services.AddOptions<MindeeOptions>().Bind(configuration.GetSection(MindeeOptions.Mindee));
            services.AddOptions<TelegramBotOptions>().Bind(configuration.GetSection(TelegramBotOptions.TelegramBot));
            services.AddOptions<OpenaiOptions>().Bind(configuration.GetSection(OpenaiOptions.Openai));

            return services;
        }

        public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
        {
            //add logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddNLog(configuration);
            });

            return services;
        }

        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            //httpClient initialization
            services.AddHttpClient(HttpClientKeywords.ClientTitle, client =>
            {
                client.BaseAddress = new Uri(HttpClientKeywords.TelegramBaseUrl);
            });

            return services;
        }
    }
}
