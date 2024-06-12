using CarInsuranceBot.Core.Options;
using CarInsuranceBot.Services.Interfaces;
using Microsoft.Extensions.Options;
using Mindee;
using Mindee.Http;
using Mindee.Input;
using Mindee.Product.Generated;
using Mindee.Product.Passport;

namespace CarInsuranceBot.Services.Services
{
    public class ParserService : IParserService
    {
        private readonly MindeeClient _mindeeClient;

        private const string EnpointName = "vehiclecard";
        private const string AccountName = "VladS";
        private const string Version = "1";

        public ParserService(IOptions<MindeeOptions> mindeeOptions)
        {
            _mindeeClient = new MindeeClient(mindeeOptions.Value.ApiKey);//or use Adapter pattern to make it more testable
        }

        public async Task<string> ParsePassportAsync(string fileUrl)
        {
            var inputSource = new UrlInputSource(fileUrl);
            var response = await _mindeeClient.ParseAsync<PassportV1>(inputSource);

            return response.Document.Inference.Prediction.ToString();
        }

        public async Task<string> ParseVehicleCardAsync(string filePath)
        {
            var inputSource = new LocalInputSource(filePath);
            //should work with file localy because of custom enpoint and EnqueueAndParseAsync doesn't have parametr for UrlInputSource
            //this product does not support synchronous mode - VladS / vehiclecard does not support sync if ParseAsync method

            var endpoint = new CustomEndpoint(
                endpointName: EnpointName,
                accountName: AccountName,
                version: Version
            );

            var response = await _mindeeClient.EnqueueAndParseAsync<GeneratedV1>(inputSource, endpoint);
            var content = response.Document.Inference.Prediction.ToString();

            return content;
        }
    }
}
