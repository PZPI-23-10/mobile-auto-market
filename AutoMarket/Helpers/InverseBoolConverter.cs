using System.Globalization;

namespace AutoMarket.Helpers
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Якщо нам прийшло число (кількість машин)
            if (value is int count)
            {
                // Повертаємо True (показувати напис "Пусто"), якщо машин 0
                return count == 0;
            }

            // Якщо прийшов булевий тип (True/False) — просто інвертуємо
            if (value is bool boolean)
            {
                return !boolean;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}