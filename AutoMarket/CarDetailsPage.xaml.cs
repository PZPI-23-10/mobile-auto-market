using AutoMarket.ViewModel;
using AutoMarket.Models;

namespace AutoMarket
{
    public partial class CarDetailsPage : ContentPage
    {
        public CarDetailsPage(CarDetailsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 1. Чекаємо трохи, поки HTML прогрузиться
            await Task.Delay(500);

            // 2. Беремо дані з ViewModel
            if (BindingContext is CarDetailsViewModel vm && vm.Listing != null)
            {
                // Перевіряємо, чи є координати (Latitude/Longitude)
                if (vm.Listing.Latitude != null && vm.Listing.Longitude != null)
                {
                    double lat = vm.Listing.Latitude.Value;
                    double lng = vm.Listing.Longitude.Value;

                    // 3. Формуємо JS команду. 
                    // Використовуємо CultureInfo, щоб числа були з крапкою (50.45), а не з комою
                    string js = $"showCarLocation({lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {lng.ToString(System.Globalization.CultureInfo.InvariantCulture)})";

                    try
                    {
                        // 4. Відправляємо команду в WebView
                        await CarMapWebView.EvaluateJavaScriptAsync(js);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Map Error: {ex.Message}");
                    }
                }
                else
                {
                    // Якщо координат немає - ховаємо карту
                    CarMapWebView.IsVisible = false;
                }
            }
        }
    }
}