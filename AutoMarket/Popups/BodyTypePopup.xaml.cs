using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class BodyTypePopup : Popup
    {
        public BodyTypePopup(List<BaseDto> items)
        {
            InitializeComponent();
            BindingContext = new { Items = items };
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.6;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.CurrentSelection.FirstOrDefault() as BaseDto;
            if (item != null) Close(item);
        }
        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}