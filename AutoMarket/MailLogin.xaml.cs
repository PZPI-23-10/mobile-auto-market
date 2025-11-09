namespace AutoMarket;
using AutoMarket.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

public partial class MailLogin : ContentPage
{
    private readonly ApiService _apiService;
    public MailLogin()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }
    private void OnPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        // Додамо перевірку, щоб уникнути NullReferenceException, якщо XAML не встиг завантажитись
        if (PasswordEntry != null)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // ★★★ ЦЕ ПРИЧИНА ВАШОЇ NULLREFERENCEEXCEPTION ★★★
        // Якщо EmailEntry або PasswordEntry null, код впаде.
        // Це означає, що ваші x:Name в XAML не прив'язалися.
        // Ця перевірка покаже вам це, замість "падіння".
        if (EmailEntry == null || PasswordEntry == null)
        {
            await DisplayAlert("Помилка XAML", "Не вдалося знайти 'EmailEntry' або 'PasswordEntry'. Перевірте x:Name у MailLogin.xaml.", "OK");
            return;
        }

        string emailInput = EmailEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(emailInput) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Помилка", "Введіть e-mail та пароль", "OK");
            return;
        }

        LoginResponse loginResult = null;
        try
        {
            loginResult = await _apiService.LoginAsync(emailInput, password);
        }
        catch (Exception apiEx)
        {
            await DisplayAlert("Помилка API", $"Не вдалося підключитися: {apiEx.Message}", "OK");
            return;
        }


        if (loginResult != null && !string.IsNullOrEmpty(loginResult.accessToken) && !string.IsNullOrEmpty(loginResult.userId))
        {
            try
            {
                await SecureStorage.SetAsync("auth_token", loginResult.accessToken);
                await SecureStorage.SetAsync("user_id", loginResult.userId);

                var (profile, profileError) = await _apiService.GetUserProfileAsync(loginResult.userId, loginResult.accessToken);

                if (profileError != null || profile == null)
                {
                    await DisplayAlert("Увага", $"Вхід виконано, але не вдалося одразу завантажити деталі профілю: {profileError}", "OK");
                }
                else
                {
                    await SecureStorage.SetAsync("user_email", profile.email);
                }

                // ★★★ ОСНОВНА ЗМІНА НОВІГАЦІЇ ★★★
                // ❌ БУЛО (створювало MainPage БЕЗ вкладок):
                // Application.Current.MainPage = new NavigationPage(new MainPage());

                // ✅ СТАЛО (замінює все на AppShell З ВКЛАДКАМИ):
                Application.Current.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка збереження/завантаження", ex.Message, "OK");
            }
        }
        else
        {
            await DisplayAlert("Помилка входу", "Невірний e-mail або пароль.", "OK");
        }
    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        // Додамо перевірку
        if (EmailEntry == null)
        {
            await DisplayAlert("Помилка XAML", "Не вдалося знайти 'EmailEntry'.", "OK");
            return;
        }

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

        // ✅ Цей код тепер ПРАЦЮЄ, тому що ми в NavigationPage (завдяки App.xaml.cs)
        await Navigation.PushAsync(new ConfirmationPage(VerificationReason.PasswordReset, userEmail));
    }
}