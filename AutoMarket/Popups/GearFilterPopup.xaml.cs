using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class GearFilterPopup : Popup
    {
        public GearFilterPopup(List<GearTypeDto> gears)
        {
            InitializeComponent();
            BindingContext = new { GearTypes = gears };

            // Висота 40%, бо пунктів мало
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.4;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as GearTypeDto;
            if (item != null)
            {
                Close(item);
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}