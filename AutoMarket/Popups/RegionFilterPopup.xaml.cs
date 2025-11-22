using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class RegionFilterPopup : Popup
    {
        public RegionFilterPopup(List<RegionDto> regions)
        {
            InitializeComponent();
            BindingContext = new { Regions = regions };

            // Налаштування розміру (60% висоти екрану)
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.6;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as RegionDto;
            if (item != null)
            {
                Close(item);
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}