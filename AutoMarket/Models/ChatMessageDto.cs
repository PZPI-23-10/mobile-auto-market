using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;

namespace AutoMarket.Models
{
    // 1. Обов'язково додаємо 'partial' і успадковуємо 'ObservableObject'
    public partial class ChatMessageDto : ObservableObject
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string Text { get; set; }
        public DateTimeOffset SentAt { get; set; }

        // 2. Видаляємо стару 'public bool IsRead { get; set; }'
        // Залишаємо тільки це поле. Бібліотека сама згенерує public IsRead.
        [ObservableProperty]
        private bool _isRead;

        [JsonIgnore]
        public bool IsMine { get; set; }
    }
}