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
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string emailInput = EmailEntry.Text; // Перейменував, щоб не плутати з email з відповіді
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(emailInput) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Помилка", "Введіть e-mail та пароль", "OK");
            return;
        }

        // 1. Робимо запит на логін
        LoginResponse loginResult = await _apiService.LoginAsync(emailInput, password);

        // 2. Перевіряємо, чи логін успішний і чи є токен/ID
        if (loginResult != null && !string.IsNullOrEmpty(loginResult.accessToken) && !string.IsNullOrEmpty(loginResult.userId))
        {
            try
            {
                // 3. Зберігаємо ТОКЕН і ID (email поки що немає)
                await SecureStorage.SetAsync("auth_token", loginResult.accessToken);
                await SecureStorage.SetAsync("user_id", loginResult.userId);

                // ✅ 4. ОДРАЗУ РОБИМО ДРУГИЙ ЗАПИТ - за профілем
                var (profile, profileError) = await _apiService.GetUserProfileAsync(loginResult.userId, loginResult.accessToken);

                // 5. Перевіряємо, чи вдалося завантажити профіль
                if (profileError != null || profile == null)
                {
                    // Якщо не вдалося - попереджаємо, але все одно можемо продовжити
                    await DisplayAlert("Увага", $"Вхід виконано, але не вдалося одразу завантажити деталі профілю: {profileError}", "OK");
                }
                else
                {
                    // ✅ 6. Якщо профіль завантажено - ЗБЕРІГАЄМО EMAIL
                    await SecureStorage.SetAsync("user_email", profile.email);
                    // Тут можна зберегти й інші дані профілю в Preferences, якщо треба
                    // Preferences.Set("user_firstName", profile.firstName); 
                }

                // 7. Переходимо на головну сторінку
                await DisplayAlert("Успіх!", "Вхід виконано.", "OK");
                Application.Current.MainPage = new NavigationPage(new ProfileEdit());

            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка збереження/завантаження", ex.Message, "OK");
            }
        }
        else
        {
            // Провал логіна
            await DisplayAlert("Помилка входу", "Невірний e-mail або пароль.", "OK");
        }
    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        string userEmail = EmailEntry.Text;
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        // 1. Валідація (у тебе це було правильно)
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

        // --- ПОЧАТОК ВИПРАВЛЕННЯ ---

        // 2. Блокуємо інтерфейс, поки йде запит
        var tapGesture = sender as View;
        if (tapGesture != null) tapGesture.IsEnabled = false;

        // 3. Викликаємо ApiService, щоб ВІДПРАВИТИ КОД
        // (Переконайся, що в тебе є метод SendPasswordResetCodeAsync в ApiService)
        bool codeSent = await _apiService.SendPasswordResetCodeAsync(userEmail);

        if (codeSent)
        {
            // 4. УСПІХ: Код відправлено, ТЕПЕР можна переходити
            await DisplayAlert("Успіх", "Код для скидання пароля відправлено на вашу пошту.", "OK");
            await Navigation.PushAsync(new ConfirmationPage(VerificationReason.PasswordReset, userEmail));
        }
        else
        {
            // 5. ПОМИЛКА: Не вдалося відправити код
            await DisplayAlert("Помилка", "Не вдалося відправити запит. Перевірте e-mail або спробуйте пізніше.", "OK");
        }

        // 6. Вмикаємо кнопку/лейбл назад
        if (tapGesture != null) tapGesture.IsEnabled = true;

        // --- КІНЕЦЬ ВИПРАВЛЕННЯ ---
    }
}