using CarInsuranceBot.Core.Const;
using CarInsuranceBot.Services.Interfaces;

namespace CarInsuranceBot.Services.Services
{
    public class FileService : IFileService
    {
        private const string FolderForDownloading = "Downloads";
        private readonly HttpClient _httpClient;

        public FileService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient(HttpClientKeywords.ClientTitle); //such implementation to increase testability
        }

        public async Task<string> DownloadFileAsync(string fileUrl, Telegram.Bot.Types.File file)
        {
            var downloadDirectory = GetDownloadDirectory();
            var filePath = Path.Combine(downloadDirectory, Path.GetFileName(file.FilePath)!);

            var response = await _httpClient.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await response.Content.CopyToAsync(fs); //dowload file to 'downloads' directory
            }

            return filePath;
        }

        private static string GetDownloadDirectory()
        {
            var userHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var downloadDirectory = Path.Combine(userHomeDirectory, FolderForDownloading);

            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            return downloadDirectory;
        }
    }
}
