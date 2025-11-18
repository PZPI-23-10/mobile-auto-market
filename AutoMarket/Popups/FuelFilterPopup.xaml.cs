using CommunityToolkit.Maui.Views;

namespace AutoMarket
{
    public partial class FuelFilterPopup : Popup
    {
        // Ми передаємо список пального СЮДИ при створенні
        public FuelFilterPopup(List<string> fuelTypes)
        {
            InitializeComponent();
            // Встановлюємо "контекст даних" pop-up на список, який ми отримали
            BindingContext = new { FuelTypes = fuelTypes };

            SetPopupSize();
        }

        private void SetPopupSize()
        {
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            double screenHeight = displayInfo.Height / displayInfo.Density;
            MainBorder.HeightRequest = screenHeight * 0.6;
            MainBorder.WidthRequest = -1;
        }

        // Коли користувач обирає елемент зі списку
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedFuel = e.CurrentSelection.FirstOrDefault() as string;
            if (selectedFuel != null)
            {
                // Закриваємо pop-up і ПОВЕРТАЄМО обране значення
                Close(selectedFuel);
            }
        }

        // Кнопка X
        private void OnCloseButtonClicked(object sender, EventArgs e) => Close(); // Повертає null

        // Кнопка "Скинути"
        private void OnClearClicked(object sender, EventArgs e) => Close(string.Empty); // Повертає порожній рядок
    }
}