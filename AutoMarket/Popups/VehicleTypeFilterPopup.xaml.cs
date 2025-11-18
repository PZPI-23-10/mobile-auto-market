using CommunityToolkit.Maui.Views;
using AutoMarket.Models; // <-- Потрібно для VehicleType

namespace AutoMarket.Popups
{
    public partial class VehicleTypeFilterPopup : Popup
    {
        // Ми передаємо список сюди при створенні
        public VehicleTypeFilterPopup(List<VehicleType> types)
        {
            InitializeComponent();
            BindingContext = new { VehicleTypes = types };
            SetPopupSize();
        }

        private void SetPopupSize()
        {
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            double screenHeight = displayInfo.Height / displayInfo.Density;
            MainBorder.HeightRequest = screenHeight * 0.6; // 60% висоти
            MainBorder.WidthRequest = -1;
        }

        // Коли користувач обрав елемент
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = e.CurrentSelection.FirstOrDefault() as VehicleType;
            if (selectedType != null)
            {
                Close(selectedType); // Повертаємо об'єкт VehicleType
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e) => Close(); // Повертає null

        // Повертаємо спеціальний рядок "CLEAR"
        private void OnClearClicked(object sender, EventArgs e) => Close("CLEAR");
    }
}