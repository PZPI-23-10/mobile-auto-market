namespace AutoMarket;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
	}
    private void OnGoogleLoginTapped(object sender, TappedEventArgs e)
    {
        DisplayAlert("Навігація", "Натиснуто вхід через Google", "OK");
    }
    private async void OnEmailLoginTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new MailLogin());
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {

        await Navigation.PushAsync(new SignUp());
    }
}