using AutoMarket.Models;
using AutoMarket.ViewModels;

namespace AutoMarket;

public partial class ChatPage : ContentPage
{
    private readonly ChatsViewModel _viewModel;

    public ChatPage(ChatsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Завантажуємо чати щоразу, коли відкриваємо вкладку
        await _viewModel.LoadChatsCommand.ExecuteAsync(null);
    }

    // Обробка кліку на чат зі списку
    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Chat selectedChat)
        {
            // Викликаємо команду з ViewModel
            await _viewModel.OpenChatCommand.ExecuteAsync(selectedChat);

            // Знімаємо виділення (щоб не світилося сірим)
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}