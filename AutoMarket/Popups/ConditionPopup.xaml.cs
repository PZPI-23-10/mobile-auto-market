using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class ConditionPopup : Popup
    {
        public ConditionPopup(List<BaseDto> items)
        {
            InitializeComponent();
            BindingContext = new { Items = items };

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.5;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is BaseDto item) Close(item);
        }
        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}