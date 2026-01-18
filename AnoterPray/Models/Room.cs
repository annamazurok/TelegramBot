using Telegram.Bot.Types;

namespace AnotherPray.Models
{
    public class Room
    {
        public string Name { get; set; }
        public string Code { get; }
        public long OwnerId { get; }
        public bool IsStarted { get; set; } = false;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public Dictionary<long, User> Users { get; } = new();

        public Room(string code, long ownerId, string name)
        {
            Code = code;
            OwnerId = ownerId;
            Name = name;
        }
    }
}
