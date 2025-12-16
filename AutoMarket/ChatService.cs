using Microsoft.AspNetCore.SignalR.Client;
using AutoMarket.Models; // Твої моделі (Chat, ChatMessage)

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