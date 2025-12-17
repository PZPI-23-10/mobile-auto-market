namespace AutoMarket.Views;

[QueryProperty(nameof(Lat), "lat")]
[QueryProperty(nameof(Lon), "lon")]
public partial class PickLocationPage : ContentPage
{
    // Параметри для прийому початкових координат (якщо редагуємо)
    public string Lat { get; set; }
    public string Lon { get; set; }

    // Змінні для збереження вибору
    private double _selectedLat;
    private double _selectedLon;

    public PickLocationPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadMap();
    }

    private void LoadMap()
    {
        // Центр за замовчуванням (Київ)
        double startLat = 50.4501;
        double startLon = 30.5234;

        // Якщо передали координати - використовуємо їх
        if (double.TryParse(Lat, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLat))
            startLat = parsedLat;

        if (double.TryParse(Lon, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedLon))
            startLon = parsedLon;

        // HTML КОД МАПИ (Leaflet)
        string htmlContent = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta name='viewport' content='width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no' />
            <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
            <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
            <style> body {{ margin: 0; padding: 0; }} #map {{ height: 100vh; width: 100vw; }} </style>
        </head>
        <body>
            <div id='map'></div>
            <script>
                var map = L.map('map').setView([{startLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {startLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}], 13);
                
                L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                    maxZoom: 19,
                    attribution: '© OpenStreetMap'
                }}).addTo(map);

                var marker;

                // Якщо є стартові координати - ставимо маркер одразу
                // marker = L.marker([{startLat}, {startLon}]).addTo(map);

                // ОБРОБКА КЛІКУ ПО МАПІ
                map.on('click', function(e) {{
                    var lat = e.latlng.lat;
                    var lng = e.latlng.lng;

                    // Видаляємо старий маркер
                    if (marker) map.removeLayer(marker);

                    // Ставимо новий
                    marker = L.marker([lat, lng]).addTo(map);

                    // ВІДПРАВЛЯЄМО ДАНІ В C# ЧЕРЕЗ ФЕЙКОВЕ ПОСИЛАННЯ
                    window.location.href = 'app://coords?lat=' + lat + '&lon=' + lng;
                }});
            </script>
        </body>
        </html>";

        var source = new HtmlWebViewSource { Html = htmlContent };
        MapWebView.Source = source;
    }

    // Слухаємо, коли JS намагається перейти за посиланням 'app://coords'
    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("app://"))
        {
            e.Cancel = true; // Скасовуємо перехід, щоб не білів екран

            // Розбираємо URL: app://coords?lat=50.123&lon=30.123
            try
            {
                var uri = new Uri(e.Url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                string latStr = query["lat"];
                string lonStr = query["lon"];

                double.TryParse(latStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _selectedLat);
                double.TryParse(lonStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _selectedLon);

                // Можна показати Toast "Точку обрано"
            }
            catch { }
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_selectedLat == 0 && _selectedLon == 0)
        {
            await DisplayAlert("Увага", "Спочатку натисніть на мапу, щоб поставити мітку!", "ОК");
            return;
        }

        // Повертаємо дані назад
        await Shell.Current.GoToAsync($"..?lat={_selectedLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={_selectedLon.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
    }
}