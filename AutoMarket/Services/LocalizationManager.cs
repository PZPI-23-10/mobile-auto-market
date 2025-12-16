using System.Globalization;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using AutoMarket.Resources.Strings;

namespace AutoMarket.Services
{
    public partial class LocalizationManager : ObservableObject
    {
        public static LocalizationManager Instance { get; } = new();

        private readonly ResourceManager _resourceManager;

        private LocalizationManager()
        {
            _resourceManager = new ResourceManager(typeof(AppResources));
        }

        // Доступ до ресурсів через {Binding [Key]}
        public string this[string key]
        {
            get => _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? $"[{key}]";
        }

        // Метод, щоб отримати переклад конкретною мовою
        public string GetString(string key, CultureInfo culture)
        {
            return _resourceManager.GetString(key, culture) ?? $"[{key}]";
        }
        // !!! ОСЬ ЦЬОГО МЕТОДУ НЕ ВИСТАЧАЛО !!!
        // Він викликається в App.xaml.cs при старті
        public void SetLanguage()
        {
            try
            {
                // Читаємо з пам'яті. Якщо там нічого немає - беремо 'uk-UA'
                string cultureName = Preferences.Get("AppLanguage", "uk-UA");

                var culture = new CultureInfo(cultureName);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
        }

        // Цей метод викликається, коли ти змінюєш мову в профілі
        public void SwitchLanguage(string cultureName)
        {
            // Зберігаємо нову мову
            Preferences.Default.Set("AppLanguage", cultureName);
            ApplyCulture(cultureName);
        }

        // Допоміжний метод, щоб не дублювати код
        private void ApplyCulture(string cultureName)
        {
            var culture = new CultureInfo(cultureName);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}