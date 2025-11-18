using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoMarket.Services;

namespace AutoMarket.ViewModels
{
    public partial class ProfilePageViewModel : ObservableObject
    {
        public ProfilePageViewModel()
        {
            // Конструктор поки що порожній
        }

        //
        // Команди для кнопок зміни мови
        //
        [RelayCommand]
        private async Task SwitchToUk()
        {
            // 1. Перемикаємо мову
            LocalizationManager.Instance.SwitchLanguage("uk-UA");

            // 2. Показуємо повідомлення (вже новою, українською мовою)
            await Shell.Current.DisplayAlert(
                LocalizationManager.Instance["LanguageChangeTitle"], // "Мову змінено"
                LocalizationManager.Instance["LanguageChangeMessage"], // "Мову... оновлено."
                "OK");
        }

        [RelayCommand]
        private async Task SwitchToEn()
        {
            // 1. Перемикаємо мову
            LocalizationManager.Instance.SwitchLanguage("en-US");

            // 2. Показуємо повідомлення (вже новою, англійською мовою)
            await Shell.Current.DisplayAlert(
                LocalizationManager.Instance["LanguageChangeTitle"], // "Language Changed"
                LocalizationManager.Instance["LanguageChangeMessage"], // "The interface language..."
                "OK");
        }
    }
}