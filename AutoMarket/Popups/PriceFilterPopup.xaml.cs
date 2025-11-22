using CommunityToolkit.Maui.Views;

namespace AutoMarket.Popups
{
    public partial class PriceFilterPopup : Popup
    {
        // Курси валют (можна взяти приблизні, бо це для фільтру)
        private const double RateUahToUsd = 41.5;
        private const double RateEurToUsd = 1.05; // 1 Євро = 1.05 Долара

        private string _currentCurrency = "USD"; // За замовчуванням долар

        public PriceFilterPopup()
        {
            InitializeComponent();

            // Розмір вікна
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.WidthRequest = (displayInfo.Width / displayInfo.Density) * 0.95;
        }

        private void OnCurrencySelected(object sender, TappedEventArgs e)
        {
            string selected = e.Parameter.ToString();
            _currentCurrency = selected;

            // Оновлюємо вигляд кнопок (хто активний - темний, інші - білі)
            UpdateButtonStyles(selected);
        }

        private void UpdateButtonStyles(string selected)
        {
            // Скидаємо стилі
            SetStyle(BtnUsd, false);
            SetStyle(BtnEur, false);
            SetStyle(BtnUah, false);

            // Активуємо обрану
            if (selected == "USD") SetStyle(BtnUsd, true);
            if (selected == "EUR") SetStyle(BtnEur, true);
            if (selected == "UAH") SetStyle(BtnUah, true);
        }

        private void SetStyle(Border btn, bool isActive)
        {
            if (isActive)
            {
                btn.BackgroundColor = Color.FromArgb("#333333"); // Темний фон
                btn.Stroke = Colors.Transparent;
                if (btn.Content is Label lbl) lbl.TextColor = Colors.White;
            }
            else
            {
                btn.BackgroundColor = Colors.White;
                btn.Stroke = Color.FromArgb("#E0E0E0");
                if (btn.Content is Label lbl) lbl.TextColor = Colors.Black;
            }
        }

        private void OnApplyClicked(object sender, EventArgs e)
        {
            // 1. Зчитуємо текст
            string textFrom = EntryFrom.Text;
            string textTo = EntryTo.Text;

            int? priceFromUsd = null;
            int? priceToUsd = null;

            // 2. Конвертуємо ВІД
            if (int.TryParse(textFrom, out int valFrom))
            {
                priceFromUsd = ConvertToUsd(valFrom);
            }

            // 3. Конвертуємо ДО
            if (int.TryParse(textTo, out int valTo))
            {
                priceToUsd = ConvertToUsd(valTo);
            }

            // 4. Повертаємо результат у доларах (int?, int?)
            Close((priceFromUsd, priceToUsd));
        }

        private int ConvertToUsd(int amount)
        {
            if (_currentCurrency == "USD") return amount;
            if (_currentCurrency == "EUR") return (int)(amount * RateEurToUsd);
            if (_currentCurrency == "UAH") return (int)(amount / RateUahToUsd);
            return amount;
        }

        private void OnCloseClicked(object sender, EventArgs e) => Close();
    }
}