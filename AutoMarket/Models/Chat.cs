using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace AutoMarket.Models
{
    // 👇 1. Клас для даних користувача всередині чату
    public class ChatUser
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string photoUrl { get; set; }
    }

    public partial class Chat : ObservableObject
    {
        public int id { get; set; }

        // 👇 2. Тепер тут об'єкти, а не int
        public ChatUser firstUser { get; set; }
        public ChatUser secondUser { get; set; }

        public DateTime createdAt { get; set; }
        public List<ChatMessageDto> messages { get; set; }

        [ObservableProperty]
        private int _unreadCount;

        // 👇 3. Додаткові поля для відображення на екрані (їх заповнимо вручну)
        [JsonIgnore]
        public string Title { get; set; } // Ім'я співрозмовника

        [JsonIgnore]
        public string ImageUrl { get; set; } // Фото співрозмовника
    }
}