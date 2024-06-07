namespace CarInsuranceBot.Services.Interfaces
{
    public interface IParserService
    {
        Task<string> ParsePassportAsync(string filePath);

        Task<string> ParseVehicleCardAsync(string filePath);
    }
}
