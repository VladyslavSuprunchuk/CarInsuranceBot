using Azure.AI.OpenAI;
using CarInsuranceBot.Core.Const;
using CarInsuranceBot.Core.Options;
using CarInsuranceBot.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CarInsuranceBot.Services.Services
{
    public class OpenaiService : IOpenaiService
    {
        private readonly OpenAIClient _aiClient;

        public OpenaiService(IOptions<OpenaiOptions> aiOptions)
        {
            _aiClient = new OpenAIClient(aiOptions.Value.ApiKey);//or use Adapter pattern to make it more testable
        }

        public async Task<string> CreateRequest(string request)
        {
            var response = await _aiClient.GetCompletionsAsync(OpenaiKeywords.ModelName, request);
            var message = response.Value.Choices.FirstOrDefault()?.Text ??
                          throw new InvalidOperationException(Errors.ChoiceNotFound);

            return message;
        }
    }
}
