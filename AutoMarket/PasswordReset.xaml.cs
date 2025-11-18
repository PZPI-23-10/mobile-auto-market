using AutoMarket.Models; // Переконайся, що модель ResetPasswordRequest тут є
using System.Diagnostics;

namespace AutoMarket;

// Я назву її 'SetNewPasswordPage', щоб не плутати з 'ChangePassword'
public partial class PasswordReset : ContentPage
{
    private readonly ApiService _apiService;
    private readonly string _email; // Поле для зберігання email

    // 1. Нам потрібен конструктор, який приймає email 
    // зі сторінки ConfirmationPage
    public PasswordReset(string email)
    {
        InitializeComponent();
        _apiService = new ApiService(); // Ініціалізуємо сервіс
        _email = email; // Зберігаємо email
    }

    private void OnNewPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        NewPasswordEntry.IsPassword = !NewPasswordEntry.IsPassword;
    }

    private void OnConfirmPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        ConfirmPasswordEntry.IsPassword = !ConfirmPasswordEntry.IsPassword;
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        string newPassword = NewPasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        // 2. Валідація (тут все було правильно)
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Помилка", "Заповніть усі поля", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Помилка", "Нові паролі не співпадають", "OK");
            return;
        }

        // 3. Створюємо об'єкт запиту (для скидання, а не зміни)
        var requestData = new ResetPasswordRequest
        {
            email = _email, // <-- ВИКОРИСТОВУЄМО EMAIL, А НЕ ТОКЕН
            password = newPassword,
            passwordConfirmation = confirmPassword
        };

        // 4. Викликаємо ApiService (метод, який я дав у минулій відповіді)
        bool success = await _apiService.ResetPasswordAsync(requestData);

        // 5. Обробляємо результат
        if (success)
        {
            await DisplayAlert("Успіх!", "Пароль успішно скинуто. Тепер ви можете увійти.", "OK");

            // Повертаємо користувача на сторінку логіну (в самий початок)
            await Navigation.PopToRootAsync();
        }
        else
        {
            await DisplayAlert("Помилка", "Не вдалося скинути пароль. Можливо, сталася помилка сервера.", "OK");
        }
    }
}