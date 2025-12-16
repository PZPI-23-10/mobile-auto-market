using System.Globalization;
using AutoMarket.Services;

namespace AutoMarket.Helpers
{
    // Цей конвертер приймає БАГАТО значень (Multi)
    public class DynamicMultiTranslateConverter : IMultiValueConverter
    {
        public static DynamicMultiTranslateConverter Instance { get; } = new DynamicMultiTranslateConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] - Це наше слово з БД (наприклад, "BENSIN")
            // values[1] - Це тригер мови (ми його не використовуємо, але він змушує конвертер запуститись знову!)

            if (values == null || values.Length == 0 || values[0] == null)
                return "";

            string serverValue = values[0].ToString().ToUpper();
            string prefix = parameter as string ?? "";

            // Формуємо ключ і перекладаємо
            string resourceKey = $"{prefix}{serverValue}";
            return LocalizationManager.Instance[resourceKey];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}