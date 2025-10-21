namespace AutoMarket;
using System.Text.RegularExpressions;
public partial class MailLogin : ContentPage
{
	public MailLogin()
	{
		InitializeComponent();
	}
    private void OnPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }

    private void OnLoginClicked(object sender, EventArgs e)
    {
        // TODO: Додати логіку входу користувача
        DisplayAlert("Вхід", "Кнопка 'Увійти' натиснута!", "OK");
    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        string userEmail = EmailEntry.Text;
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";




        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await DisplayAlert("Помилка", "Будь ласка, введіть e-mail", "OK");
            return;
        }


        if (!Regex.IsMatch(userEmail, emailPattern))
        {
            await DisplayAlert("Помилка", "Введіть коректну адресу пошти", "OK");
            return;
        }


        await Navigation.PushAsync(new ConfirmationPage(VerificationReason.PasswordReset, userEmail));
    }
}