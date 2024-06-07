using CarInsuranceBot.Core.Const;
using CarInsuranceBot.Core.Options;
using CarInsuranceBot.Services.Interfaces;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CarInsuranceBot.Services.Services
{
    public class BotCommunicationService : IBotCommunicationService
    {
        private readonly TelegramBotClient _telegramBot;
        private readonly IParserService _parserService;
        private readonly IFileService _fileService;
        private readonly IOpenaiService _openaiService;
        private readonly TelegramBotOptions _telegramBotOptions;

        public BotCommunicationService(
            IParserService parserService,
            IFileService fileService,
            IOpenaiService openaiService,
            IOptions<TelegramBotOptions> telegramBotOptions)
        {
            _telegramBotOptions = telegramBotOptions.Value;
            _telegramBot = new TelegramBotClient(_telegramBotOptions.Token);
            _parserService = parserService;
            _fileService = fileService;
            _openaiService = openaiService;
        }

        public async Task StartAsync()
        {
            await _telegramBot.ReceiveAsync(OnMessageAsync, HandleErrorAsync);

            Thread.Sleep(-1); // equals endless cycle
        }

        private async Task OnMessageAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            if (update.Message is Message message)
            {
                if (message.Type == MessageType.Text)
                {
                    var (feedback, replyMarkup) = await ProcessTextMessageAsync(message.Text!);

                    await _telegramBot.SendTextMessageAsync(message.Chat.Id, feedback, replyMarkup: replyMarkup);
                }

                if (message.Type == MessageType.Photo)
                {
                    var file = await _telegramBot.GetFileAsync(message.Photo[message.Photo.Count() - 1].FileId);
                    var feedback = await AnalyzePhotoMessageAsync(message.Caption, file);

                    await _telegramBot.SendTextMessageAsync(message.Chat.Id, feedback);
                }
            }

            else 
            if (update.CallbackQuery is CallbackQuery callbackQuery)
            {
                var (feedback, replyMarkup) = await ProcessTextMessageAsync(callbackQuery.Data!);

                await _telegramBot.SendTextMessageAsync(callbackQuery!.Message!.Chat.Id, feedback, replyMarkup: replyMarkup);
            }
        }

        //generating a text message with related bottons
        private async Task<(string feedback, InlineKeyboardMarkup markup)> ProcessTextMessageAsync(string message)
        {
            var (feedback, tag) = await AnalyzeTextMessageAsync(message);
            var replyMarkup = GetInlineKeyboardButtons(tag);

            return (feedback, replyMarkup);
        }

        //genarating a text message
        private async Task<(string feedback, string tag)> AnalyzeTextMessageAsync(string message)
        {
            var feedback = message switch
            {
                BotKeywords.StartTeg => (Messages.Welcome, BotKeywords.StartTeg),
                BotKeywords.ConfirmDocumentsTeg => (Messages.DocumentConfirmed, BotKeywords.ConfirmDocumentsTeg),
                BotKeywords.ReshareDocumentsTeg => (Messages.NoPhotoDescription, BotKeywords.ReshareDocumentsTeg),
                BotKeywords.PaymentDeclinedTeg => (Messages.PaymentDeclined, BotKeywords.PaymentDeclinedTeg),
                BotKeywords.PaymentConfirmTeg => (await _openaiService.CreateRequest($"{OpenaiKeywords.FillTemplateRequest} {OpenaiKeywords.PolicyInsuranceTemplate}"),
                                                  BotKeywords.PaymentConfirmTeg),
                BotKeywords.FinishTeg => (Messages.Finish, BotKeywords.FinishTeg),
                _ => (Messages.InvalidCommand, Messages.InvalidCommand)
            };

            return feedback;
        }

        //photo processing
        private async Task<string> AnalyzePhotoMessageAsync(string? caption, Telegram.Bot.Types.File file)
        {
            var feedback = string.Empty;

            if (string.IsNullOrEmpty(caption))
            {
                feedback = Messages.NoPhotoDescription;

                return feedback;
            }

            var downloadUrl = $"{HttpClientKeywords.TelegramBaseUrl}{_telegramBotOptions.Token}/{file.FilePath}";

            switch (caption.ToLower())
            {
                case BotKeywords.PassportCaption:
                    feedback = await _parserService.ParsePassportAsync(downloadUrl);
                    break;

                case BotKeywords.VehicleCardCaption:
                    var filePath = await _fileService.DownloadFileAsync(downloadUrl, file);
                    feedback = await _parserService.ParseVehicleCardAsync(filePath);
                    break;

                default:
                    break;
            }

            return feedback;
        }

        //receiving buttons related to message
        private InlineKeyboardMarkup GetInlineKeyboardButtons(string tag)
        {
            return tag switch
            {
                BotKeywords.StartTeg => new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Confirm data from documents", BotKeywords.ConfirmDocumentsTeg),
                    InlineKeyboardButton.WithCallbackData("Reshare data from documents", BotKeywords.ReshareDocumentsTeg)
                }),
                BotKeywords.ConfirmDocumentsTeg => new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Confirm payment", BotKeywords.PaymentConfirmTeg),
                    InlineKeyboardButton.WithCallbackData("Decline payment", BotKeywords.PaymentDeclinedTeg)
                }),
                BotKeywords.PaymentConfirmTeg => new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("Generate more documents", BotKeywords.StartTeg),
                    InlineKeyboardButton.WithCallbackData("Finish the process", BotKeywords.FinishTeg)
                }),
                _ => new InlineKeyboardMarkup(Enumerable.Empty<InlineKeyboardButton[]>())
            }; ;
        }

        private async Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                Console.WriteLine(apiRequestException.ToString());
            }
        }
    }
}
