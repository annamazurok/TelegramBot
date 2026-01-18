using AnotherPray.Models;
using Telegram.Bot.Types;

namespace AnotherPray.UI
{
    public static class BotMessages
    {
        public const string CreateRoomButton = "Створити кімнату";
        public const string StartButtonText = "▶ Запуск";

        // Привітання
        public const string Welcome = "Привіт! Я тут, щоб допомогти тобі створити молитовний ланцюжок\n😌🤌";

        // Повідомлення про створення кімнати
        public const string RoomNameEmpty = "Назва кімнати не може бути порожньою 🤔. Спробуй ще раз:";
        public const string AskRoomName = "Будь ласка, введи назву кімнати\n👉👈:";
        public static string RoomCreated(string name, string code, int time) =>
            $"🔥Є! Кімната створена\n\n" +
            $"Назва: `{name}`\n\n" +
            $"Код: `{code}`\n\n" +
            $"⏳ Час на приєднання: {time}хв\n\n" +
            $"Запроси інших за кнопкою або передай код 👌.";
        public const string StartRoomPrompt = "Коли всі приєднаються, запусти ланцюжок ⚡";

        // Повідомлення при приєднанні
        public static string JoinedRoom(string roomName) => $"🙌 Вітаю! Ти приєднався до кімнати `{roomName}` 😎";
        public static string AlreadyJoinedRoom(string roomName) => $"😌 Ти вже перебуваєш у кімнаті `{roomName}` ⚡";
        public static string UserJoinedOwnerNotification(string userName, Room room) =>
            $"👤 *{userName}* приєднався до кімнати `{room.Name}`";
        public static string UserJoinedCountOwnerNotification(Room room) =>
            $"👥 Учасників зараз: {room.Users.Count}\n(разом з тобою 😉)";
        public const string UsersCountButton = "👥 Кількість учасників";

        // Помилки
        public const string NotOwner = "😶 Хм… ти не власник цієї кімнати 🤐";
        public const string RoomNotFound = "😶 Упс! Кімнату з таким кодом не знайдено";
        public const string ChainAlreadyStarted = "⏳ Ланцюжок вже запущено. Приєднання закрите 😶";
        public const string MinParticipants = "⚡ Потрібно мінімум 2 учасники для старту 😶";
        public const string UnknownError = "❌ Ой… щось пішло не так 🤐";

        // Ланцюжок
        public static string SecretFriend(string name) =>
            $"🙏 Твій таємний друг на цей тиждень:\n\n👉 {name} ✨\n\nОбов'язково напиши та поцікався молитовними потребами 😌 Бажаю мати гарне спілкування 🤗";

        public const string ChainStarted = "🔥 Ланцюжок створено! Повідомлення надіслані 🙏🤗";
        public const string ChainReady = "✨ Тепер можеш створити нову кімнату 😉";
        public const string EnterRoomNamePrompt = "😌 Напиши назву кімнати, щоб почати ⚡:";

        public const string Cancel = "❌ Скасувати";
        public const string CreationCancelled = "Створення кімнати скасовано 🤐.";

        public const string DeleteRoomButton = "🗑️ Видалити кімнату";
        public const string RoomDeleted = "❌ Упс.. Здається, власник видалив кімнату 😶.";
        public const string ConfirmDelete = "⚠️ Ти точно хочеш видалити кімнату?";
        public const string ConfirmYes = "✅ Так, видалити";
        public const string ConfirmNo = "❌ Ні";
        public const string DeleteCancelled = "Видалення скасовано.";

        public const string RoomExpired = "⏰ Тік-так! Кімната автоматично закрита. Наступного разу спробуй швидше 😌\nТа пам'ятай, що неактивні кімнати варто видаляти 😉";

        public const string LeaveRoomButton = "🚪 Покинути кімнату";
        public const string LeftRoom = "🚪 Ти покинув(ла) кімнату.";
        public static string UserLeftOwnerNotification(string userName, string roomName) =>
            $"👋 {userName} покинув(ла) кімнату «{roomName}»";
    }
}
