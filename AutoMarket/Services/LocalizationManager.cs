using System.ComponentModel;
using System.Globalization;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel; // У вас вже є цей пакет
using AutoMarket.Resources.Strings; // Шлях до ваших AppResources

namespace AutoMarket.Services
{
    // Ми використовуємо ObservableObject, щоб UI міг "бачити" зміни
    public partial class LocalizationManager : ObservableObject
    {
        // Це "Singleton" - єдиний екземпляр на весь додаток
        public static LocalizationManager Instance { get; } = new();

        private readonly ResourceManager _resourceManager;

        private LocalizationManager()
        {
            // Вказуємо, де шукати наші файли .resx
            _resourceManager = new ResourceManager(typeof(AppResources));
        }

        // Ця властивість дозволить нам у XAML писати:
        // <Label Text="{Binding [SearchButtonText], Source={...}}"/>
        public string this[string key]
        {
            get => _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]";
        }

        // Встановлює початкову мову при запуску
        public void SetLanguage()
        {
            try
            {
                string cultureName = Preferences.Default.Get("AppLanguage", "uk-UA");
                var culture = new CultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set language: {ex.Message}");
            }
        }


        // ГОЛОВНИЙ МЕТОД - перемикає мову
        public void SwitchLanguage(string cultureName)
        {
            // 1. Зберігаємо вибір
            Preferences.Default.Set("AppLanguage", cultureName);

            // 2. Встановлюємо культуру
            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // 3. ЦЕ МАГІЯ:
            // Ми сповіщаємо ВЕСЬ інтерфейс, що всі переклади
            // потрібно оновити "на льоту".
            OnPropertyChanged(string.Empty); // "string.Empty" означає "оновити ВСЕ"
        }
    }
}