using AutoMarket.ViewModel;

namespace AutoMarket.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel _viewModel;

    public FavoritesPage(FavoritesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel; // Зв'язуємо сторінку з ViewModel
    }

    // Цей метод спрацьовує щоразу, коли ти заходиш на вкладку
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadFavoritesAsync(); // <--- ОСЬ ЦЕ ЗАПУСКАЄ ЗАВАНТАЖЕННЯ
    }
}