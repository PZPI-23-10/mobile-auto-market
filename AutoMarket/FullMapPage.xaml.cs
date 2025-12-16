/*using System.Windows.Input;

namespace AutoMarket;

public partial class FullMapPage : ContentPage, IQueryAttributable
{
    private double _lat;
    private double _lng;
    public ICommand BackCommand { get; }
    
    public FullMapPage()
    {
        InitializeComponent();
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    // 👇 Цей метод спіймає твій Dictionary
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("lat")) _lat = Convert.ToDouble(query["lat"]);
        if (query.ContainsKey("lng")) _lng = Convert.ToDouble(query["lng"]);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(10000);
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            // Робимо стрілку видимою
            IsVisible = true,

            // Вішаємо на неї конкретну команду "Йди назад"
            Command = new Command(async () =>
            {
                try
                {
                    await Shell.Current.GoToAsync("..");
                }
                catch
                {
                    // Якщо стандартний шлях ".." не спрацює, пробуємо абсолютний шлях назад
                    await Shell.Current.Navigation.PopAsync();
                }
            })
        });
        // Формуємо JS з крапкою
        string latStr = _lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lngStr = _lng.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string js = $"showCarLocation({latStr}, {lngStr});";
        // Якщо координати 0 (не прийшли), ставимо центр України, щоб карта не була сірою
        if (_lat == 0 && _lng == 0)
        {
            js = "map.setView([49.0, 31.0], 6);";
        }
        else
        {
            js = $"showCarLocation({latStr}, {lngStr});";
        }

        string enableInteractionJs = "map.dragging.enable(); map.touchZoom.enable(); map.doubleClickZoom.enable(); map.scrollWheelZoom.enable(); map.boxZoom.enable(); map.keyboard.enable(); if (map.tap) map.tap.enable();";

        try
        {
            await FullMapView.EvaluateJavaScriptAsync(js);
            await FullMapView.EvaluateJavaScriptAsync(enableInteractionJs);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map Error: {ex.Message}");
        }
    }
    
    


    private async void OnZoomInClicked(object sender, EventArgs e)
    {
        await FullMapView.EvaluateJavaScriptAsync("map.zoomIn();");
    }

    private async void OnZoomOutClicked(object sender, EventArgs e)
    {
        await FullMapView.EvaluateJavaScriptAsync("map.zoomOut();");
    }
}*/


using System.Globalization;
using System.Windows.Input;

namespace AutoMarket;

public partial class FullMapPage : ContentPage, IQueryAttributable
{
    private double _lat;
    private double _lng;
    private bool _isMapLoaded = false; // Прапор: чи завантажився HTML

    public ICommand BackCommand { get; }

    public FullMapPage()
    {
        InitializeComponent();
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    // 1. Ловимо координати
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("lat")) _lat = Convert.ToDouble(query["lat"]);
        if (query.ContainsKey("lng")) _lng = Convert.ToDouble(query["lng"]);

        // Пробуємо оновити карту (якщо вона вже завантажена з минулого разу)
        UpdateMap();
    }

    // 2. Цей метод має викликатися з XAML, коли HTML догрузився
    private async void OnMapNavigated(object sender, WebNavigatedEventArgs e)
    {
        _isMapLoaded = true;
        await UpdateMap();
    }

    // 3. Основна логіка оновлення
    private async Task UpdateMap()
    {
        // Якщо карта ще не готова (HTML не прогрузився) — чекаємо
        if (!_isMapLoaded) return;

        // Формуємо JS
        string latStr = _lat.ToString(CultureInfo.InvariantCulture);
        string lngStr = _lng.ToString(CultureInfo.InvariantCulture);
        string js;

        // Якщо координати прийшли (не нулі)
        if (_lat != 0 && _lng != 0)
        {
            js = $"showCarLocation({latStr}, {lngStr});";
        }
        else
        {
            // Дефолтний центр (Україна)
            js = "map.setView([49.0, 31.0], 6);";
        }

        string enableInteractionJs = "map.dragging.enable(); map.touchZoom.enable(); map.doubleClickZoom.enable(); map.scrollWheelZoom.enable(); map.boxZoom.enable(); map.keyboard.enable(); if (map.tap) map.tap.enable();";

        try
        {
            System.Diagnostics.Debug.WriteLine($"SENDING JS: {js}");
            await FullMapView.EvaluateJavaScriptAsync(js);
            await FullMapView.EvaluateJavaScriptAsync(enableInteractionJs);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Map Error: {ex.Message}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Примусова логіка для кнопки "Назад" (без затримки)
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = true,
            Command = new Command(async () =>
            {
                try
                {
                    await Shell.Current.GoToAsync("..");
                }
                catch
                {
                    await Shell.Current.Navigation.PopAsync();
                }
            })
        });
    }

    private async void OnZoomInClicked(object sender, EventArgs e)
    {
        await FullMapView.EvaluateJavaScriptAsync("map.zoomIn();");
    }

    private async void OnZoomOutClicked(object sender, EventArgs e)
    {
        await FullMapView.EvaluateJavaScriptAsync("map.zoomOut();");
    }
}