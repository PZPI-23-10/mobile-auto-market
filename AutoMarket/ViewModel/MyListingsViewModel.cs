using AutoMarket.Models;
using AutoMarket.Services;
using AutoMarket.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AutoMarket.ViewModel
{
    public partial class MyListingsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        public ObservableCollection<CarListing> MyListings { get; } = new();

        [ObservableProperty]
        private bool isBusy;

        public MyListingsViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                MyListings.Clear();

                // Викликаємо наш новий метод
                var cars = await _apiService.GetUserListingsAsync();

                foreach (var car in cars)
                {
                    MyListings.Add(car);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OpenDetails(CarListing listing)
        {
            if (listing == null) return;
            var navParam = new Dictionary<string, object> { { "Listing", listing } };

            // Правильно (зберігає кнопку Назад):
            await Shell.Current.GoToAsync(nameof(CarDetailsPage), navParam);
        }

        [RelayCommand]
        private async Task DeleteListing(CarListing listing)
        {
            if (listing == null) return;

            // 1. Питаємо підтвердження
            bool confirm = await Shell.Current.DisplayAlert(
                "Видалення",
                $"Ви дійсно хочете видалити авто?",
                "Так, видалити", "Скасувати");

            if (!confirm) return;

            // 2. Видаляємо через API
            IsBusy = true;
            bool success = await _apiService.DeleteListingAsync(listing.Id);
            IsBusy = false;

            if (success)
            {
                // 3. Якщо успіх - прибираємо зі списку на екрані
                MyListings.Remove(listing);
                await Shell.Current.DisplayAlert("Успіх", "Оголошення видалено.", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Помилка", "Не вдалося видалити оголошення.", "OK");
            }
        }

    }
}