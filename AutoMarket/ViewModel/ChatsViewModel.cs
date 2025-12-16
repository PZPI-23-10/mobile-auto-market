using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AutoMarket.Models;
using AutoMarket.Services;
using System.Diagnostics;

namespace AutoMarket.ViewModels
{
    public partial class ChatsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<Chat> _chats; // Список чатів

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRefreshing;

        public ChatsViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _chats = new ObservableCollection<Chat>();
        }

        [RelayCommand]
        public async Task LoadChatsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                string token = await SecureStorage.GetAsync("auth_token");
                string myIdStr = await SecureStorage.GetAsync("user_id"); // 👇 Беремо свій ID

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(myIdStr))
                {
                    await Shell.Current.GoToAsync($"//{nameof(Login)}");
                    return;
                }

                int myId = int.Parse(myIdStr);

                // Отримуємо список чатів з сервера
                var chatsList = await _apiService.GetMyChatsAsync(token);

                Chats.Clear();

                foreach (var chat in chatsList)
                {
                    // --- 👇 НОВА ЛОГІКА: ВИЗНАЧАЄМО ІМ'Я ТА ФОТО СПІВРОЗМОВНИКА ---

                    ChatUser otherUser = null;

                    // Якщо "перший" юзер - це я, значить співрозмовник - "другий"
                    if (chat.firstUser != null && chat.firstUser.id == myId)
                    {
                        otherUser = chat.secondUser;
                    }
                    else
                    {
                        // Інакше співрозмовник - "перший"
                        otherUser = chat.firstUser;
                    }

                    // Заповнюємо дані для відображення (Title та ImageUrl)
                    if (otherUser != null)
                    {
                        chat.Title = $"{otherUser.firstName} {otherUser.lastName}";

                        // Якщо фото є - беремо його, якщо ні - ставимо заглушку
                        chat.ImageUrl = !string.IsNullOrEmpty(otherUser.photoUrl)
                                        ? otherUser.photoUrl
                                        : "profile_icon.png";
                    }
                    else
                    {
                        chat.Title = "Невідомий користувач";
                        chat.ImageUrl = "profile_icon.png";
                    }
                    // --- 👆 КІНЕЦЬ НОВОЇ ЛОГІКИ ---


                    // Отримуємо кількість непрочитаних (як і було)
                    var count = await _apiService.GetUnreadCountAsync(chat.id, token);
                    chat.UnreadCount = count;

                    Chats.Add(chat);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка завантаження чатів: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        public async Task OpenChatAsync(Chat chat)
        {
            if (chat == null) return;

            // Важливо: переконайся, що в тебе в AppShell зареєстровано "ConversationPage"
            await Shell.Current.GoToAsync("ConversationPage", new Dictionary<string, object>
            {
                { "CurrentChat", chat } // Передаємо чат під ключем "CurrentChat"
            });
        }

        [RelayCommand]
        public async Task CreateTestChat()
        {
            try
            {
                string token = await SecureStorage.GetAsync("auth_token");
                int otherUserId = 17; // ID продавця (для тесту)

                IsLoading = true;

                var newChat = await _apiService.GetOrCreateChatAsync(otherUserId, token);

                if (newChat != null)
                {
                    await Shell.Current.DisplayAlert("Успіх", $"Чат створено! ID: {newChat.id}", "OK");
                    await LoadChatsAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Помилка", "Не вдалося створити чат.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Критична помилка", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}