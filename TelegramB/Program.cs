
using System.Globalization;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramB;


string token = "";
string url = $"https://api.telegram.org/bot{token}";
TelegramBotClientOptions options = new TelegramBotClientOptions(token: token, baseUrl: url);

var botClient = new TelegramBotClient(options:options);

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    string userMessage = "hello";

    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { Text: { } messageText } message)
        return;
    // Only process text messages

    var chatId = message.Chat.Id;
    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
    
    switch (messageText.ToLower())
    {
        case "время":
            userMessage = DateTime.Now.TimeOfDay.ToString("hh\\:mm");
            // Echo received message text
            await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: userMessage,
               cancellationToken: cancellationToken);
            break;
        case "дата":
            userMessage = DateTime.Now.Date.ToShortDateString();
            // Echo received message text
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: userMessage,
                cancellationToken: cancellationToken);
            break;
        case "погода":
            Task<string> res = new Weather(new RestClient("http://api.weatherapi.com/v1")).GetWeather();
            userMessage = res.Result + "°C";
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: userMessage,
                cancellationToken: cancellationToken);
            break;

        default:
            userMessage = ".";
            break;
    }


}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
