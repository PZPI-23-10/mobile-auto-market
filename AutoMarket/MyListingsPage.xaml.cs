using AutoMarket.ViewModel;

namespace AutoMarket.Views;

public partial class MyListingsPage : ContentPage
{
    private readonly MyListingsViewModel _viewModel;

    // Прибираємо IServiceProvider, він тут більше не треба
    public MyListingsPage(MyListingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Просто вантажимо дані при відкритті
        if (_viewModel.MyListings.Count == 0)
        {
            await _viewModel.LoadDataAsync();
        }
    }
}