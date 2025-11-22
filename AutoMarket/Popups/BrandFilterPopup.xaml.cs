using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class BrandFilterPopup : Popup
    {
        public BrandFilterPopup(List<VehicleBrandDto> brands)
        {
            InitializeComponent();
            BindingContext = new { Brands = brands };

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.7; // 70% екрану
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as VehicleBrandDto;
            if (item != null) Close(item);
        }

        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}