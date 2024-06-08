using CarInsuranceBot.Core.Const;
using CarInsuranceBot.Core.Options;
using CarInsuranceBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<BotCommunicationService> _loger;

        public BotCommunicationService(
            IParserService parserService,
            IFileService fileService,
            IOpenaiService openaiService,
            IOptions<TelegramBotOptions> telegramBotOptions,
            ILogger<BotCommunicationService> loger)
        {
            _telegramBotOptions = telegramBotOptions.Value;
            _telegramBot = new TelegramBotClient(_telegramBotOptions.Token);
            _parserService = parserService;
            _fileService = fileService;
            _openaiService = openaiService;
            _loger = loger;
        }

        public async Task StartAsync()
        {
            await _telegramBot.ReceiveAsync(OnMessageAsync, HandleErrorAsync);
        }

        private async Task OnMessageAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is Message message)
                {
                    await ProcessUpdateMessageAsync(message);
                }
                else
                if (update.CallbackQuery is CallbackQuery callbackQuery)
                {
                    await ProcessCallbackQueryAsync(callbackQuery);
                }
            }  
            catch (Exception ex) 
            {
                string userId = string.Empty;

                if (update.Message?.Chat.Id != null)
                {
                    userId = update.Message.Chat.Id.ToString();
                }
                else if (update.CallbackQuery?.Message?.Chat.Id != null)
                {
                    userId = update.CallbackQuery.Message.Chat.Id.ToString();
                }

                _loger.LogError(ex, Errors.ProcessingErrorMessage, userId);
            }
        }

        //processing message type
        private async Task ProcessUpdateMessageAsync(Message message)
        {
            if (message.Type == MessageType.Text)
            {
                var (feedback, replyMarkup) = await ProcessTextMessageAsync(message.Text!);

                await _telegramBot.SendTextMessageAsync(message.Chat.Id, feedback, replyMarkup: replyMarkup);
                return;
            }

            if (message.Type == MessageType.Photo)
            {
                var file = await _telegramBot.GetFileAsync(message.Photo![message.Photo.Count() - 1].FileId);
                var feedback = await AnalyzePhotoMessageAsync(message.Caption, file);

                await _telegramBot.SendTextMessageAsync(message.Chat.Id, feedback);
                return;
            }

            await _telegramBot.SendTextMessageAsync(message.Chat.Id, Messages.InvalidCommand);
        }

        //processing callback type
        private async Task ProcessCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            var (feedback, replyMarkup) = await ProcessTextMessageAsync(callbackQuery.Data!);

            await _telegramBot.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, feedback, replyMarkup: replyMarkup);
        }

        //generating a text message with related buttons
        private async Task<(string feedback, InlineKeyboardMarkup markup)> ProcessTextMessageAsync(string message)
        {
            var (feedback, tag) = await AnalyzeTextMessageAsync(message);
            var replyMarkup = GetInlineKeyboardButtons(tag);

            return (feedback, replyMarkup);
        }

        //genarating a text message
        private async Task<(string feedback, string tag)> AnalyzeTextMessageAsync(string message)
        {
            //1 line switch
            var feedback = message switch
            {
                BotKeywords.StartTeg => (Messages.Welcome, BotKeywords.StartTeg),
                BotKeywords.ConfirmDocumentsTeg => (Messages.DocumentConfirmed, BotKeywords.ConfirmDocumentsTeg),
                BotKeywords.ReshareDocumentsTeg => (Messages.NoPhotoDescription, BotKeywords.ReshareDocumentsTeg),
                BotKeywords.PaymentDeclinedTeg => (Messages.PaymentDeclined, BotKeywords.PaymentDeclinedTeg),
                BotKeywords.PaymentConfirmTeg => (await _openaiService.CreateRequest($"{OpenaiKeywords.FillTemplateRequest} {OpenaiKeywords.PolicyInsuranceTemplate}"),
                                                  BotKeywords.PaymentConfirmTeg),
                BotKeywords.FinishTeg => (Messages.Finish, BotKeywords.FinishTeg),
                _ => ((await _openaiService.CreateRequest($"{OpenaiKeywords.LiveChattingRequest}: {message}"), BotKeywords.ChattingTeg)) //live chatting to related topics as additional feature
            };

            return feedback;
        }

        //processing photo
        private async Task<string> AnalyzePhotoMessageAsync(string? caption, Telegram.Bot.Types.File file)
        {
            var feedback = string.Empty;

            if (string.IsNullOrEmpty(caption))
            {
                feedback = Messages.NoPhotoDescription;

                return feedback;
            }

            var downloadUrl = $"{HttpClientKeywords.TelegramBaseUrl}{_telegramBotOptions.Token}/{file.FilePath}";

            //2 line switch so use more older version to make it more readable
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
                    InlineKeyboardButton.WithCallbackData(BotKeywords.ConfirmDocumentsButton, BotKeywords.ConfirmDocumentsTeg),
                    InlineKeyboardButton.WithCallbackData(BotKeywords.ReshareDocumentsButton, BotKeywords.ReshareDocumentsTeg)
                }),
                BotKeywords.ConfirmDocumentsTeg => new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData(BotKeywords.PaymentConfirmButton, BotKeywords.PaymentConfirmTeg),
                    InlineKeyboardButton.WithCallbackData(BotKeywords.PaymentDeclinedButton, BotKeywords.PaymentDeclinedTeg)
                }),
                BotKeywords.PaymentConfirmTeg => new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData(BotKeywords.RestartButton, BotKeywords.StartTeg),
                    InlineKeyboardButton.WithCallbackData(BotKeywords.FinishButton, BotKeywords.FinishTeg)
                }),
                _ => new InlineKeyboardMarkup(Enumerable.Empty<InlineKeyboardButton[]>())
            }; ;
        }

        private Task HandleErrorAsync(
             ITelegramBotClient botClient,
             Exception exception,
             CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:[{apiRequestException.ErrorCode}]- {apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
