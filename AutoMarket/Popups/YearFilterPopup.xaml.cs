using CommunityToolkit.Maui.Views;

namespace AutoMarket
{
    public partial class YearFilterPopup : Popup
    {
        public YearFilterPopup()
        {
            InitializeComponent();
            SetPopupSize();

            // Викликаємо наш новий метод для створення списків
            PopulateYearLists();
        }

        private void SetPopupSize()
        {
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            // Отримуємо "справжні" розміри екрану
            double screenHeight = displayInfo.Height / displayInfo.Density;
            double screenWidth = displayInfo.Width / displayInfo.Density; // <-- Додаємо ширину

            // Встановлюємо висоту ТА ШИРИНУ на 100%
            MainBorder.HeightRequest = screenHeight * 0.79;
            MainBorder.WidthRequest = screenWidth; // <-- ДОДАЙТЕ ЦЕЙ РЯДОК
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        // ==========================================================
        //     НОВИЙ МЕТОД: СТВОРЕННЯ СПИСКІВ РОКІВ
        // ==========================================================
        private void PopulateYearLists()
        {
            // Отримуємо поточний рік
            int currentYear = DateTime.Now.Year;

            // Генеруємо роки, наприклад, до 1980 (або як вам потрібно)
            // Ідемо у зворотному порядку, щоб новіші роки були зверху
            for (int year = currentYear; year >= 1980; year--)
            {
                // 1. Створюємо рядок для колонки "Від"
                var fromRow = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
                fromRow.Children.Add(new RadioButton
                {
                    GroupName = "FromYear", // <-- Унікальна група
                    Value = year
                });
                fromRow.Children.Add(new Label
                {
                    Text = year.ToString(),
                    FontSize = 18,
                    VerticalOptions = LayoutOptions.Center
                });

                // Додаємо рядок "Від" у відповідний список в XAML
                FromYearList.Children.Add(fromRow);

                // 2. Створюємо рядок для колонки "До"
                var toRow = new HorizontalStackLayout { Spacing = 10, VerticalOptions = LayoutOptions.Center };
                toRow.Children.Add(new RadioButton
                {
                    GroupName = "ToYear", // <-- Інша унікальна група
                    Value = year
                });
                toRow.Children.Add(new Label
                {
                    Text = year.ToString(),
                    FontSize = 18,
                    VerticalOptions = LayoutOptions.Center
                });

                // Додаємо рядок "До" у відповідний список в XAML
                ToYearList.Children.Add(toRow);
            }
        }
    }
}