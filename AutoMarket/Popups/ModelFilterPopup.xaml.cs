using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class ModelFilterPopup : Popup
    {
        public ModelFilterPopup(List<VehicleModelDto> models)
        {
            InitializeComponent();
            BindingContext = new { Models = models };

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            double screenWidth = displayInfo.Width / displayInfo.Density;
            double screenHeight = displayInfo.Height / displayInfo.Density;
            MainBorder.HeightRequest = screenHeight * 0.7;
            MainBorder.WidthRequest = screenWidth;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as VehicleModelDto;
            if (item != null) Close(item);
        }
        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}