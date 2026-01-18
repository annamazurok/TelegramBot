using AnotherPray.Models;
using AnotherPray.UI;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AnotherPray.Services
{
    public class RoomService
    {
        private readonly Dictionary<string, Room> _rooms = new();

        public Room CreateRoom(long ownerId, User owner, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Room name cannot be empty", nameof(name));

            var code = Guid.NewGuid().ToString("N")[..6].ToUpper();
            var room = new Room(code, ownerId, name.Trim());

            room.Users.Add(ownerId, owner);
            _rooms[code] = room;

            return room;
        }

        public JoinResult TryJoinRoom(string code, User user)
        {
            if (!_rooms.TryGetValue(code, out var room))
                return JoinResult.RoomNotFound;

            if (room.IsStarted)
                return JoinResult.RoomAlreadyStarted;

            if (room.Users.ContainsKey(user.Id))
                return JoinResult.AlreadyJoined;

            room.Users.TryAdd(user.Id, user);
            return JoinResult.Success;
        }

        public Room? GetRoomByOwner(long ownerId)
            => _rooms.Values.FirstOrDefault(r => r.OwnerId == ownerId);

        public Room? GetRoomByCode(string code)
            => _rooms.TryGetValue(code, out var room) ? room : null;

        public async Task<bool> StartPrayerChainAsync(
            Room room,
            ITelegramBotClient bot,
            CancellationToken ct)
        {
            if (room.Users.Count < 2)
                return false;

            room.IsStarted = true;

            var shuffled = room.Users.Values
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            for (int i = 0; i < shuffled.Count; i++)
            {
                var from = shuffled[i];
                var to = shuffled[(i + 1) % shuffled.Count];

                await bot.SendMessage(
                    from.Id,
                    BotMessages.SecretFriend(to.FirstName),
                    cancellationToken: ct);
            }

            foreach (var member in room.Users.Values)
            {
                await bot.SendMessage(
                    member.Id,
                    BotMessages.ChainReady,
                    replyMarkup: Keyboards.MainMenu(),
                    cancellationToken: ct);
            }
            _rooms.Remove(room.Code);
            return true;
        }

        public bool DeleteRoom(long ownerId)
        {
            var room = GetRoomByOwner(ownerId);
            if (room == null)
                return false;

            _rooms.Remove(room.Code);
            return true;
        }

        public async Task AutoCloseRoomAsync(
            Room room,
            ITelegramBotClient bot,
            TimeSpan timeout,
            CancellationToken ct)
        {
            await Task.Delay(timeout, ct);

            if (!_rooms.ContainsKey(room.Code)) return;
            if (room.IsStarted) return;

            _rooms.Remove(room.Code);

            foreach (var member in room.Users.Values)
            {
                await bot.SendMessage(
                    member.Id,
                    BotMessages.RoomExpired,
                    replyMarkup: Keyboards.MainMenu(),
                    cancellationToken: ct);
            }
        }

        public bool LeaveRoom(long userId, out Room? room)
        {
            room = _rooms.Values.FirstOrDefault(r => r.Users.ContainsKey(userId));
            if (room == null)
                return false;

            if (room.OwnerId == userId)
                return false;

            return room.Users.Remove(userId);
        }
    }
}
