using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoMarket.Services;

namespace AutoMarket.ViewModels
{
    public partial class ProfilePageViewModel : ObservableObject
    {
        public ProfilePageViewModel()
        {
            // Конструктор
        }

        // --- ЛОГІКА ВИХОДУ ---


        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Питаємо підтвердження, щоб користувач не вийшов випадково
            bool answer = await Shell.Current.DisplayAlert("Вихід", "Ви дійсно хочете вийти з профілю?", "Так", "Ні");

            if (answer)
            {
                // Викликаємо статичний метод очищення токена
                App.Logout();
            }
        }

        // --- ЛОГІКА МОВИ ---

        [RelayCommand]
        private async Task SwitchToUk()
        {
            // 1. Просто зберігаємо налаштування (НЕ міняємо мову в додатку прямо зараз)
            Preferences.Set("AppLanguage", "uk-UA");

            // 2. Показуємо повідомлення
            await Application.Current.MainPage.DisplayAlert(
                "Зміна мови",
                "Мову змінено на Українську. Будь ласка, перезапустіть додаток, щоб зміни набули чинності.",
                "ОК");
        }

        [RelayCommand]
        private async Task SwitchToEn()
        {
            // 1. Зберігаємо налаштування
            Preferences.Set("AppLanguage", "en-US");

            // 2. Показуємо повідомлення
            await Application.Current.MainPage.DisplayAlert(
                "Language Change",
                "Language changed to English. Please restart the application to apply changes.",
                "OK");
        }



    }
}