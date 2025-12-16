using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AutoMarket.Models;
using AutoMarket.Views;
using CommunityToolkit.Mvvm.Messaging;
using AutoMarket.Messages;

namespace AutoMarket.ViewModel
{
    public partial class FavoritesViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public ObservableCollection<CarListing> FavoriteListings { get; } = new();

        [ObservableProperty]
        private bool isBusy;

        public FavoritesViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadFavoritesAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                FavoriteListings.Clear();
                var cars = await _apiService.GetFavoriteListingsAsync();

                foreach (var car in cars)
                {
                    car.IsFavorite = true;
                    FavoriteListings.Add(car);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // 👇👇👇 ОСЬ ТУТ БУЛА ПОМИЛКА 👇👇👇

        [RelayCommand] // 1. Додаємо атрибут, щоб XAML бачив команду
        public async Task OpenDetails(CarListing listing) // 2. Назва OpenDetails -> створиться OpenDetailsCommand
        {
            if (listing == null) return;

            // Створюємо параметри для навігації
            var navigationParameter = new Dictionary<string, object>
            {
                { "Listing", listing }
            };

            // Переходимо на сторінку деталей
            // Переконайся, що CarDetailsPage зареєстрована в маршрутах (AppShell)
            await Shell.Current.GoToAsync(nameof(CarDetailsPage), navigationParameter);
        }

        [RelayCommand]
        public async Task RemoveFromFavorites(CarListing car)
        {
            bool confirm = await Shell.Current.DisplayAlert("Видалення", "Видалити з обраного?", "Так", "Ні");
            if (!confirm) return;

            bool success = await _apiService.RemoveFromFavoritesAsync(car.Id);
            if (success)
            {
                FavoriteListings.Remove(car);

                WeakReferenceMessenger.Default.Send(new FavoriteChangeMessage(car.Id, false));
            }
        }
    }
}