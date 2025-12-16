using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AutoMarket.Models;
using AutoMarket.Services;

namespace AutoMarket.ViewModels
{
    [QueryProperty(nameof(CurrentChat), "ChatData")] // Отримуємо чат при переході
    public partial class ConversationViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ApiService _apiService;
        private readonly ChatHubService _chatHubService;

        [ObservableProperty]
        private Chat _currentChat;

        [ObservableProperty]
        private ObservableCollection<ChatMessageDto> _messages;

        [ObservableProperty]
        private string _newMessageText;

        [ObservableProperty]
        private int _myUserId;

        public ConversationViewModel(ApiService apiService, ChatHubService chatHubService)
        {
            _apiService = apiService;
            _chatHubService = chatHubService;
            _messages = new ObservableCollection<ChatMessageDto>();
        }
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Шукаємо ключ "CurrentChat", який ми передали з CarDetailsViewModel
            if (query.ContainsKey("CurrentChat") && query["CurrentChat"] is Chat chat)
            {
                CurrentChat = chat;

                // Можна одразу запустити завантаження повідомлень, якщо треба
                // Але зазвичай OnAppearing це зробить, якщо CurrentChat вже не null
            }
        }
       /* public async Task InitializeAsync()
        {
            // 1. Отримуємо токен і ID юзера
            string token = await SecureStorage.GetAsync("auth_token");
            string userIdStr = await SecureStorage.GetAsync("user_id");
            if (int.TryParse(userIdStr, out int uid)) MyUserId = uid;

            if (CurrentChat != null)
            {
                // === ВАНТАЖИМО ІСТОРІЮ ===
                var history = await _apiService.GetChatHistoryAsync(CurrentChat.id, token);

                Messages.Clear();
                foreach (var msg in history)
                {
                    msg.IsMine = msg.SenderId == MyUserId;
                    Messages.Add(msg);
                }
                // ===========================

                // 2. Підключаємося до SignalR
                await _chatHubService.ConnectAsync(token);
                await _chatHubService.JoinChatGroupAsync(CurrentChat.id);
                await _chatHubService.MarkAsReadAsync(CurrentChat.id);

                // 👇 ДОДАЙ ЦІ ДВА РЯДКИ (ВІДПИСКА):
                // Це гарантує, що старі "зомбі-підписки" видаляться перед тим, як створити нову
                _chatHubService.OnMessagesRead -= HandleMessagesRead;
                _chatHubService.OnMessageReceived -= ReceiveMessage;

                // А тепер підписуємося (як було)
                _chatHubService.OnMessagesRead += HandleMessagesRead;
                _chatHubService.OnMessageReceived += ReceiveMessage;
            }
        }*/

        public async Task InitializeAsync()
        {
            // 1. Отримуємо токен і ID юзера (це вже було)
            string token = await SecureStorage.GetAsync("auth_token");
            string userIdStr = await SecureStorage.GetAsync("user_id");
            if (int.TryParse(userIdStr, out int uid)) MyUserId = uid;

            if (CurrentChat != null)
            {
                // 👇👇👇 ДОДАЙ ЦЕЙ БЛОК 👇👇👇
                // Якщо ми зайшли з оголошення, Title і ImageUrl можуть бути пусті.
                // Заповнюємо їх тут, бо ми вже знаємо MyUserId.
                if (string.IsNullOrEmpty(CurrentChat.Title))
                {
                    ChatUser otherUser = null;

                    // Перевіряємо, хто є хто
                    if (CurrentChat.firstUser != null && CurrentChat.firstUser.id == MyUserId)
                    {
                        otherUser = CurrentChat.secondUser;
                    }
                    else
                    {
                        otherUser = CurrentChat.firstUser;
                    }

                    // Заповнюємо інфу
                    if (otherUser != null)
                    {
                        CurrentChat.Title = $"{otherUser.firstName} {otherUser.lastName}";
                        CurrentChat.ImageUrl = !string.IsNullOrEmpty(otherUser.photoUrl)
                                                ? otherUser.photoUrl
                                                : "profile_icon.png";
                    }
                    else
                    {
                        CurrentChat.Title = "Невідомий користувач";
                        CurrentChat.ImageUrl = "profile_icon.png";
                    }

                    // 🔥 ВАЖЛИВО: Повідомляємо UI, що об'єкт CurrentChat змінився, 
                    // щоб текст на екрані оновився
                    OnPropertyChanged(nameof(CurrentChat));
                }



                var history = await _apiService.GetChatHistoryAsync(CurrentChat.id, token);

                Messages.Clear();
                foreach (var msg in history)
                {
                    msg.IsMine = msg.SenderId == MyUserId;
                    Messages.Add(msg);
                }
                // ===========================

                // 2. Підключаємося до SignalR
                await _chatHubService.ConnectAsync(token);
                await _chatHubService.JoinChatGroupAsync(CurrentChat.id);
                await _chatHubService.MarkAsReadAsync(CurrentChat.id);

                // 👇 ДОДАЙ ЦІ ДВА РЯДКИ (ВІДПИСКА):
                // Це гарантує, що старі "зомбі-підписки" видаляться перед тим, як створити нову
                _chatHubService.OnMessagesRead -= HandleMessagesRead;
                _chatHubService.OnMessageReceived -= ReceiveMessage;

                // А тепер підписуємося (як було)
                _chatHubService.OnMessagesRead += HandleMessagesRead;
                _chatHubService.OnMessageReceived += ReceiveMessage;
            }
        }
        private void HandleMessagesRead(int chatId)
        {
            // Перевіряємо, чи це стосується поточного чату
            if (CurrentChat == null || CurrentChat.id != chatId) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Проходимось по всіх повідомленнях і ставимо галочки
                foreach (var msg in Messages)
                {
                    if (msg.IsMine && !msg.IsRead) // Тільки мої і ще не прочитані
                    {
                        msg.IsRead = true;
                        // Трюк для оновлення UI (щоб XAML побачив зміну)
                        // Можна використати OnPropertyChanged, але найпростіше 
                        // просто замінити об'єкт або використати CommunityToolkit ObservableObject на DTO

                        // В твоєму випадку, якщо DTO не Observable, UI може не оновитись миттєво.
                        // Якщо галочки не зміняться самі, напиши, я дам фікс.
                    }
                }
                var tempMessages = new ObservableCollection<ChatMessageDto>(Messages);
                Messages = tempMessages;
            });
        }
        private void ReceiveMessage(ChatMessageDto message)
        {
            // Додаємо повідомлення в список (в головному потоці UI)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                message.IsMine = message.SenderId == MyUserId;
                Messages.Add(message);
            });
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(NewMessageText) || CurrentChat == null) return;

            // Відправляємо через SignalR
            await _chatHubService.SendMessageAsync(CurrentChat.id, NewMessageText);

            // Очищаємо поле вводу
            NewMessageText = string.Empty;
        }

        // При закритті сторінки - відписуємося
        public void Cleanup()
        {
            _chatHubService.OnMessageReceived -= ReceiveMessage;
            _chatHubService.OnMessagesRead -= HandleMessagesRead;
        }
    }
}