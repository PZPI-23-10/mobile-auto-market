using CommunityToolkit.Maui.Views;

namespace AutoMarket.Popups
{
    public partial class SingleYearPopup : Popup
    {
        public List<int> Years { get; set; } = new();

        public SingleYearPopup()
        {
            InitializeComponent();
            int currentYear = DateTime.Now.Year + 1;
            for (int i = currentYear; i >= 1980; i--) Years.Add(i);

            BindingContext = this;

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.6;
            MainBorder.WidthRequest = -1;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is int year) Close(year);
        }
        private void OnCloseButtonClicked(object sender, EventArgs e) => Close();
    }
}