using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class FuelFilterPopup : Popup
    {
        public FuelFilterPopup(List<FuelTypeDto> fuels)
        {
            InitializeComponent();

            // Встановлюємо дані для списку
            BindingContext = new { FuelTypes = fuels };

            // Налаштовуємо висоту (40%
            // екрану)
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.4;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as FuelTypeDto;
            if (item != null)
            {
                // Повертаємо вибраний елемент і закриваємо
                Close(item);
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}