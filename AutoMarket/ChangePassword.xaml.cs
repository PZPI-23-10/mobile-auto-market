using AutoMarket.Models;
using System.Diagnostics;


namespace AutoMarket;



public partial class ChangePassword : ContentPage
{
    private readonly ApiService _apiService; // ✅ Додали сервіс
    public ChangePassword()
    { 
		InitializeComponent();
        _apiService = new ApiService();
    }
    private void OnCurrentPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        CurrentPasswordEntry.IsPassword = !CurrentPasswordEntry.IsPassword;
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
        string currentPassword = CurrentPasswordEntry.Text;
        string newPassword = NewPasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        // 2. Валідація
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Помилка", "Заповніть усі поля", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await DisplayAlert("Помилка", "Нові паролі не співпадають", "OK");
            return;
        }

        // Тут можна додати перевірку складності нового пароля, якщо треба

        // 3. Дістаємо токен
        string token = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("Помилка", "Сесія не знайдена.", "OK");
            return;
        }

        // 4. Створюємо об'єкт запиту
        var requestData = new ChangePasswordRequest
        {
            Password = currentPassword,
            NewPassword = newPassword,
            PasswordConfirmation = confirmPassword
        };

        // 5. Викликаємо ApiService
        string errorResult = await _apiService.ChangePasswordAsync(requestData, token);

        // 6. Обробляємо результат
        if (errorResult == null)
        {
            await DisplayAlert("Успіх!", "Пароль успішно змінено.", "OK");
            // Можливо, повернутися на попередню сторінку
            await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlert("Помилка зміни пароля", errorResult, "OK");
        }
    }
}