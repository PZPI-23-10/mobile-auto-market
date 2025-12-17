using CommunityToolkit.Maui.Views;
using AutoMarket.Models;

namespace AutoMarket.Popups
{
    public partial class VehicleTypeFilterPopup : Popup
    {
        // Ми передаємо вже підготовлений список (з перекладеними назвами)
        public VehicleTypeFilterPopup(List<VehicleTypeDto> types)
        {
            InitializeComponent();

            // Прив'язуємо дані
            BindingContext = new { VehicleTypes = types };

            // Налаштовуємо розмір (шторка знизу)
            SetPopupSize();
        }

        private void SetPopupSize()
        {
            // Отримуємо розміри екрану
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            // Конвертуємо пікселі в одиниці виміру MAUI
            double density = displayInfo.Density;
            double screenHeight = displayInfo.Height / density;
            double screenWidth = displayInfo.Width / density;

            // Встановлюємо висоту 50-60% від екрану
            MainBorder.HeightRequest = screenHeight * 0.6;

            // Ширина на весь екран
            MainBorder.WidthRequest = screenWidth;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = e.CurrentSelection.FirstOrDefault() as VehicleTypeDto;

            if (selectedType != null)
            {
                // Закриваємо Popup і повертаємо обраний об'єкт
                Close(selectedType);
            }
        }

        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            // Просто закриваємо, повертаємо null
            Close(null);
        }
    }
}