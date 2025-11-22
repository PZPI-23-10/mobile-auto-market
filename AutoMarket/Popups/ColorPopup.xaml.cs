using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class ColorPopup : Popup
    {
        public ColorPopup(List<ColorDto> colors)
        {
            InitializeComponent();
            BindingContext = new { Colors = colors };
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.6;
            MainBorder.WidthRequest = -1;
        }
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ColorDto item) Close(item);
        }
        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}