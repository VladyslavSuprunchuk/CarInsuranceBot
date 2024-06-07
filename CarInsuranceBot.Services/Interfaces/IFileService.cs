namespace CarInsuranceBot.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> DownloadFileAsync(string fileUrl, Telegram.Bot.Types.File file);
    }
}
