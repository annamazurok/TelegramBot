using Telegram.Bot.Types.ReplyMarkups;

namespace AnotherPray.UI
{
    public static class Keyboards
    {
        public static ReplyKeyboardMarkup MainMenu() =>
            new(new[]
            {
            new KeyboardButton[] { BotMessages.CreateRoomButton }
            })
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup StartButton() =>
            new(new[]
            {
            new KeyboardButton[] { BotMessages.StartButtonText }
            })
            { ResizeKeyboard = true };

        public static InlineKeyboardMarkup JoinLink(string botUsername, string code, string name)
        {
            return new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl(
                    $"🙌 Приєднатись до кімнати {name}",
                    $"https://t.me/{botUsername}?start={code}"
                )
            );
        }

        public static ReplyKeyboardMarkup CancelOnly() =>
            new(new[]
            {
                new KeyboardButton[] { BotMessages.Cancel }
            })
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup OwnerRoomMenu() =>
            new(new[]
            {
                new KeyboardButton[] { BotMessages.StartButtonText },
                new KeyboardButton[] { BotMessages.UsersCountButton },
                new KeyboardButton[] { BotMessages.DeleteRoomButton }
            })
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup ConfirmDeleteMenu() =>
            new(new[]
            {
                new KeyboardButton[] { BotMessages.ConfirmYes },
                new KeyboardButton[] { BotMessages.ConfirmNo }
            })
            { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup ParticipantRoomMenu() =>
            new(new[]
            {
                new KeyboardButton[] { BotMessages.LeaveRoomButton }
            })
            { ResizeKeyboard = true };
    }
}
