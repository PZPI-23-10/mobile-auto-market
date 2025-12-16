using System.Globalization;
using AutoMarket.Services;

namespace AutoMarket.Helpers
{
    public class DynamicTranslateConverter : IValueConverter
    {
        // !!! ДОДАЙ ЦЕЙ РЯДОК (СТАТИЧНИЙ ЕКЗЕМПЛЯР) !!!
        public static DynamicTranslateConverter Instance { get; } = new DynamicTranslateConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            string serverValue = value.ToString().ToUpper();
            string prefix = parameter as string ?? "";
            string resourceKey = $"{prefix}{serverValue}";

            // !!! ЯДЕРНИЙ ВАРІАНТ !!!
            // Ми не довіряємо потоку. Ми читаємо збережену мову з пам'яті.
            string savedLanguage = Preferences.Get("AppLanguage", "uk-UA");
            var explicitCulture = new CultureInfo(savedLanguage);

            // Отримуємо переклад саме для цієї культури
            // (Для цього нам треба трошки підправити LocalizationManager, див. нижче)
            return LocalizationManager.Instance.GetString(resourceKey, explicitCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}