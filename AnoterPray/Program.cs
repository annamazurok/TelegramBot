using AnotherPray.Handlers;
using AnotherPray.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;

var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN")
    ?? throw new Exception("BOT_TOKEN is not set");

var bot = new TelegramBotClient(botToken);
var roomService = new RoomService();
var handler = new UpdateHandler(bot, roomService);

using var cts = new CancellationTokenSource();

bot.StartReceiving(
    async (client, update, ct) =>
    {
        await handler.HandleAsync(update, ct);
    },
    async (client, exception, ct) =>
    {
        Console.WriteLine(exception.Message);
    },
    cancellationToken: cts.Token
);

Console.WriteLine("Бот запущений і працює");

await Task.Delay(Timeout.Infinite, cts.Token);