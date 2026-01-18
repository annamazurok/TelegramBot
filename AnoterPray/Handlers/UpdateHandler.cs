using AnotherPray.Models;
using AnotherPray.Services;
using AnotherPray.UI;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace AnotherPray.Handlers
{
    public class UpdateHandler
    {
        private readonly ITelegramBotClient _bot;
        private readonly RoomService _roomService;
        private readonly HashSet<long> _awaitingRoomName = new();
        private readonly HashSet<long> _awaitingDeleteConfirm = new();
        private const int time = 30; 

        public UpdateHandler(ITelegramBotClient bot, RoomService roomService)
        {
            _bot = bot;
            _roomService = roomService;
        }

        public async Task HandleAsync(Update update, CancellationToken ct)
        {
            if (update.Message is not { Text: { } text }) return;

            var chatId = update.Message.Chat.Id;
            var user = update.Message.From!;
            text = text.Trim();

            if (_awaitingRoomName.Contains(user.Id))
            {
                if (text == BotMessages.Cancel)
                {
                    _awaitingRoomName.Remove(user.Id);

                    await _bot.SendMessage(
                        chatId,
                        BotMessages.CreationCancelled,
                        replyMarkup: Keyboards.MainMenu(),
                        cancellationToken: ct);
                    return;
                }

                _awaitingRoomName.Remove(user.Id);
                try
                {
                    var room = _roomService.CreateRoom(user.Id, user, text);

                    _ = _roomService.AutoCloseRoomAsync(
                        room,
                        _bot,
                        TimeSpan.FromMinutes(time),
                        ct);

                    await _bot.SendMessage(
                    chatId,
                    BotMessages.RoomCreated(room.Name, room.Code, time),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    replyMarkup: Keyboards.JoinLink("anotherpathway_pray_bot", room.Code, room.Name),
                    cancellationToken: ct);

                    await _bot.SendMessage(
                        chatId,
                        BotMessages.StartRoomPrompt,
                        replyMarkup: Keyboards.OwnerRoomMenu(),
                        cancellationToken: ct);
                    return;
                }
                catch (ArgumentException)
                {
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.RoomNameEmpty,
                        cancellationToken: ct);
                    _awaitingRoomName.Add(user.Id);
                }
            }

            if (text.StartsWith("/start"))
            {
                var parts = text.Split(' ');

                if (parts.Length == 2)
                {
                    var code = parts[1];
                    var result = _roomService.TryJoinRoom(code, user);
                    var room = _roomService.GetRoomByCode(code);

                    await HandleJoinResult(chatId, user, result, room, ct);
                    return;
                }

                await _bot.SendMessage(
                    chatId,
                    BotMessages.Welcome,
                    replyMarkup: Keyboards.MainMenu(),
                    cancellationToken: ct);
                return;
            }

            if (text == BotMessages.CreateRoomButton)
            {
                _awaitingRoomName.Add(user.Id);
                await _bot.SendMessage(
                    chatId,
                    BotMessages.AskRoomName,
                    replyMarkup: Keyboards.CancelOnly(),
                    cancellationToken: ct);
                return;
            }

            if (text == BotMessages.DeleteRoomButton)
            {
                var room = _roomService.GetRoomByOwner(user.Id);
                if (room == null)
                {
                    await _bot.SendMessage(chatId, BotMessages.NotOwner, cancellationToken: ct);
                    return;
                }

                _awaitingDeleteConfirm.Add(user.Id);

                await _bot.SendMessage(
                    chatId,
                    BotMessages.ConfirmDelete,
                    replyMarkup: Keyboards.ConfirmDeleteMenu(),
                    cancellationToken: ct);
                return;
            }

            if (_awaitingDeleteConfirm.Contains(user.Id))
            {
                if (text == BotMessages.ConfirmNo)
                {
                    _awaitingDeleteConfirm.Remove(user.Id);

                    await _bot.SendMessage(
                        chatId,
                        BotMessages.DeleteCancelled,
                        replyMarkup: Keyboards.OwnerRoomMenu(),
                        cancellationToken: ct);
                    return;
                }

                if (text == BotMessages.ConfirmYes)
                {
                    _awaitingDeleteConfirm.Remove(user.Id);

                    var room = _roomService.GetRoomByOwner(user.Id);
                    if (room == null) return;

                    foreach (var member in room.Users.Values)
                    {
                        await _bot.SendMessage(
                            member.Id,
                            BotMessages.RoomDeleted,
                            replyMarkup: Keyboards.MainMenu(),
                            cancellationToken: ct);
                    }

                    _roomService.DeleteRoom(user.Id);
                    return;
                }
            }

            var joinResult = _roomService.TryJoinRoom(text, user);

            if (joinResult != JoinResult.RoomNotFound)
            {
                var room = _roomService.GetRoomByCode(text);
                await HandleJoinResult(chatId, user, joinResult, room, ct);
                return;
            }

            if (text == BotMessages.StartButtonText)
            {
                var room = _roomService.GetRoomByOwner(user.Id);
                if (room == null)
                {
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.NotOwner,
                        cancellationToken: ct);
                    return;
                }

                var success = await _roomService.StartPrayerChainAsync(room, _bot, ct);
                if (!success)
                {
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.MinParticipants,
                        cancellationToken: ct);
                    return;
                }

                await _bot.SendMessage(
                    chatId,
                    BotMessages.ChainStarted,
                    cancellationToken: ct);
            }

            if (text == BotMessages.UsersCountButton)
            {
                var room = _roomService.GetRoomByOwner(user.Id);
                if (room == null)
                {
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.NotOwner,
                        cancellationToken: ct);
                    return;
                }
                await _bot.SendMessage(
                    chatId,
                    BotMessages.UserJoinedCountOwnerNotification(room),
                    cancellationToken: ct);
                return;
            }

            if (text == BotMessages.LeaveRoomButton)
            {
                var success = _roomService.LeaveRoom(user.Id, out var room);

                if (!success || room == null)
                {
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.UnknownError,
                        replyMarkup: Keyboards.MainMenu(),
                        cancellationToken: ct);
                    return;
                }

                await _bot.SendMessage(
                    room.OwnerId,
                    BotMessages.UserLeftOwnerNotification(
                        user.FirstName,
                        room.Name),
                    cancellationToken: ct);

                await _bot.SendMessage(
                    chatId,
                    BotMessages.LeftRoom,
                    replyMarkup: Keyboards.MainMenu(),
                    cancellationToken: ct);
                return;
            }
        }

        private async Task HandleJoinResult(
            long chatId,
            User user,
            JoinResult result,
            Room? room,
            CancellationToken ct)
        {
            switch (result)
            {
                case JoinResult.Success:
                    if (room != null && room.OwnerId != user.Id)
                    {
                        await _bot.SendMessage(
                            room.OwnerId,
                            BotMessages.UserJoinedOwnerNotification(user.FirstName, room),
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct);
                    }
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.JoinedRoom(room?.Name),
                        parseMode: ParseMode.Markdown,
                        replyMarkup: room?.OwnerId == user.Id
                            ? Keyboards.OwnerRoomMenu()
                            : Keyboards.ParticipantRoomMenu(),
                        cancellationToken: ct);
                    break;

                case JoinResult.AlreadyJoined:
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.AlreadyJoinedRoom(room?.Name),
                        parseMode: ParseMode.Markdown,
                        replyMarkup: room?.OwnerId == user.Id
                            ? Keyboards.OwnerRoomMenu()
                            : Keyboards.ParticipantRoomMenu(),
                        cancellationToken: ct);
                    break;

                case JoinResult.RoomAlreadyStarted:
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.ChainAlreadyStarted,
                        replyMarkup: Keyboards.MainMenu(),
                        cancellationToken: ct);
                    break;

                case JoinResult.RoomNotFound:
                    await _bot.SendMessage(
                        chatId,
                        BotMessages.RoomNotFound,
                        cancellationToken: ct);
                    break;

                default:
                    await _bot.SendMessage(chatId, BotMessages.UnknownError);
                    break;
            }
        }
    }
}
