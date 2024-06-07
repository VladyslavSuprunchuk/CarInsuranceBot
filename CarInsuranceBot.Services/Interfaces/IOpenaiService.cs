namespace CarInsuranceBot.Services.Interfaces
{
    public interface IOpenaiService
    {
        Task<string> CreateRequest(string request);
    }
}
