using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class CityFilterPopup : Popup
    {
        public CityFilterPopup(List<CityDto> cities)
        {
            InitializeComponent();
            BindingContext = new { Cities = cities };

            // Налаштування розміру (70% висоти, бо міст може бути багато)
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.7;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as CityDto;
            if (item != null)
            {
                Close(item);
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}