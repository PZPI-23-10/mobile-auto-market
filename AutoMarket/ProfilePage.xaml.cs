namespace AutoMarket;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}

    private async void OnSettingsTapped(object sender, TappedEventArgs e)
    {
        // ВИКОНУЄМО АСИНХРОННИЙ ПЕРЕХІД
        // 'ProfileEdit' - це наш маршрут, який ми зареєструємо нижче.
        await Shell.Current.GoToAsync("ProfileEdit");
    }

}