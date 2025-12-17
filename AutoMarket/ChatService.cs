using AutoMarket.Models; // Твої моделі (Chat, ChatMessage)
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;

public class ChatHubService
{
    private readonly string _hubUrl = "https://backend-auto-market-wih5h.ondigitalocean.app/hubs/chat";
    private HubConnection _hubConnection;

    // Подія, щоб повідомляти UI про нові повідомлення
    public event Action<ChatMessageDto> OnMessageReceived;
    public event Action<int> OnMessagesRead;
    public async Task ConnectAsync(string token)
    {
        // 1. Налаштовуємо підключення
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        // 2. "Слухаємо" вхідні повідомлення
        _hubConnection.On<ChatMessageDto>("ReceiveMessage", (message) =>
        {
            OnMessageReceived?.Invoke(message);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Перевіряємо, чи ми ЗАРАЗ не в чаті
                    var currentPage = Shell.Current?.CurrentPage;

                    // Якщо назва сторінки НЕ "ChatPage" (або як вона у вас називається)
                    // Тоді показуємо сповіщення
                    bool isChatOpen = currentPage != null && currentPage.GetType().Name.Contains("Chat");

                    if (!isChatOpen)
                    {
                        var request = new NotificationRequest
                        {
                            NotificationId = new Random().Next(1000, 9999),

                            // 👇 ВИПРАВЛЕННЯ ТУТ 👇
                            // SenderId - це число, тому просто пишемо статичний текст, 
                            // або форматуємо рядок: $"Від: {message.SenderId}"
                            Title = "Нове повідомлення",

                            // Тут використовуємо Text, бо у твоїй моделі поле називається Text
                            Description = message.Text ?? "Вам написали",

                            BadgeNumber = 1,
                            Android = new AndroidOptions
                            {
                                // IconSmallName = { ResourceName = "message_icon" } 
                            }
                        };

                        await LocalNotificationCenter.Current.Show(request);
                    }
                }
                catch (Exception ex)
                {
                    // Щоб не крашнуло, якщо щось піде не так
                    System.Diagnostics.Debug.WriteLine($"Notification Error: {ex.Message}");
                }
            });

        });

        // 👇 НОВЕ 2: Слухаємо сигнал від сервера "MessagesRead"
        // Коли співрозмовник прочитав чат, сервер надішле сюди ID чату
        _hubConnection.On<int>("MessagesRead", (chatId) =>
        {
            OnMessagesRead?.Invoke(chatId);
        });

        // 3. Відкриваємо з'єднання
        await _hubConnection.StartAsync();
    }
    public async Task MarkAsReadAsync(int chatId)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("MarkAsRead", chatId);
        }
    }
    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }

    // Метод: "Я зайшов у чат №5" (JoinChat з бекенду)
    public async Task JoinChatGroupAsync(int chatId)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("JoinChat", chatId);
        }
    }

    // Метод: "Відправити повідомлення" (SendMessage з бекенду)
    public async Task SendMessageAsync(int chatId, string text)
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SendMessage", chatId, text);
        }
    }
}