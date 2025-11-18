using CommunityToolkit.Maui.Views;

namespace AutoMarket
{
    public partial class PriceFilterPopup : Popup
    {
        public PriceFilterPopup()
        {
            InitializeComponent();
            SetPopupSize();
        }

        private void SetPopupSize()
        {
            // 1. Отримуємо інформацію про екран (розміри, щільність пікселів)
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            // 2. Обчислюємо "справжню" висоту екрану в незалежних одиницях.
            //    (ділимо пікселі на щільність)
            double screenHeight = displayInfo.Height / displayInfo.Density;
            double screenWidth = displayInfo.Width / displayInfo.Density;

            // 3. Встановлюємо висоту нашого Border (який ми назвали MainBorder в XAML)
            //    на 75% від висоти екрану.
            MainBorder.HeightRequest = screenHeight * 0.79;
            MainBorder.WidthRequest = screenWidth;
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        // ==========================================================
        //     НОВИЙ МЕТОД: ЛОГІКА ПЕРЕМИКАННЯ ВАЛЮТ
        // ==========================================================
        private void OnCurrencyClicked(object sender, EventArgs e)
        {
            // 1. Спочатку скидаємо всі кнопки до "неактивного" стилю
            ResetCurrencyButtons();

            // 2. Визначаємо, яка кнопка була натиснута
            if (sender is Button clickedButton)
            {
                // 3. Встановлюємо "активний" стиль для натиснутої кнопки
                clickedButton.BackgroundColor = Color.FromHex("#333");
                clickedButton.TextColor = Colors.White;

                // $ та € мають бути жирним шрифтом, грн. - ні
                if (clickedButton == UsdButton || clickedButton == EurButton)
                {
                    clickedButton.FontAttributes = FontAttributes.Bold;
                }
            }
        }

        // Допоміжний метод, який скидає стилі всіх кнопок
        private void ResetCurrencyButtons()
        {
            // Стиль для $
            UsdButton.BackgroundColor = Color.FromHex("#EEE");
            UsdButton.TextColor = Colors.Black;
            UsdButton.FontAttributes = FontAttributes.Bold; // $ завжди жирний

            // Стиль для €
            EurButton.BackgroundColor = Color.FromHex("#EEE");
            EurButton.TextColor = Colors.Black;
            EurButton.FontAttributes = FontAttributes.Bold; // € завжди жирний

            // Стиль для грн.
            UahButton.BackgroundColor = Color.FromHex("#EEE");
            UahButton.TextColor = Colors.Black;
            UahButton.FontAttributes = FontAttributes.None; // грн. не жирний
        }
    }
}