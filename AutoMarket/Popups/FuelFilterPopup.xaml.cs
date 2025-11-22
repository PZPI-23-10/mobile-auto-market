using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class FuelFilterPopup : Popup
    {
        public FuelFilterPopup(List<FuelTypeDto> fuels)
        {
            InitializeComponent();
            BindingContext = new { FuelTypes = fuels };

            // Висота десь 40%, бо видів пального небагато (Бензин, Дизель, Газ, Електро...)
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.4;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as FuelTypeDto;
            if (item != null)
            {
                Close(item);
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}