using CommunityToolkit.Maui.Views;

namespace AutoMarket.Popups
{
    public partial class YearFilterPopup : Popup
    {
        public List<int> Years { get; set; } = new();

        private int? _selectedFrom;
        private int? _selectedTo;

        public YearFilterPopup()
        {
            InitializeComponent();

            // Генеруємо роки: від 2026 вниз до 1980
            int currentYear = DateTime.Now.Year + 1; // 2026
            for (int i = currentYear; i >= 1980; i--)
            {
                Years.Add(i);
            }

            BindingContext = this;

            // Розмір на весь екран майже
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            MainBorder.HeightRequest = (displayInfo.Height / displayInfo.Density) * 0.85;
            MainBorder.WidthRequest = -1;
        }

        private void OnFromSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Зберігаємо вибір "Від"
            if (e.CurrentSelection.FirstOrDefault() is int year)
            {
                _selectedFrom = year;
            }
        }

        private void OnToSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Зберігаємо вибір "До"
            if (e.CurrentSelection.FirstOrDefault() is int year)
            {
                _selectedTo = year;
            }
        }

        private void OnApplyClicked(object sender, EventArgs e)
        {
            // Повертаємо об'єкт з двома значеннями
            // (Використовуємо кортеж / tuple)
            Close((_selectedFrom, _selectedTo));
        }

        private void OnCloseClicked(object sender, EventArgs e) => Close();
    }
}