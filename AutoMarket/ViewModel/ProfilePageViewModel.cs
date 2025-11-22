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
            LocalizationManager.Instance.SwitchLanguage("uk-UA");
            await ShowLanguageAlert();
        }

        [RelayCommand]
        private async Task SwitchToEn()
        {
            LocalizationManager.Instance.SwitchLanguage("en-US");
            await ShowLanguageAlert();
        }

        private async Task ShowLanguageAlert()
        {
            await Shell.Current.DisplayAlert(
                LocalizationManager.Instance["LanguageChangeTitle"],
                LocalizationManager.Instance["LanguageChangeMessage"],
                "OK");
        }
    }
}