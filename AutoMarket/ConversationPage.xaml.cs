using AutoMarket.ViewModels;

namespace AutoMarket;

public partial class ConversationPage : ContentPage
{
    private readonly ConversationViewModel _viewModel;

    public ConversationPage(ConversationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup(); // Відключаємося від подій, коли виходимо
    }
}