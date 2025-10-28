using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace AutoMarket
{
    public enum VerificationReason
    {
        EmailConfirmation,
        PasswordReset
    }

    public partial class ConfirmationPage : ContentPage
    {
        private readonly VerificationReason _reason;
        private readonly string _email;
        private readonly ApiService _apiService;
        public ConfirmationPage(VerificationReason reason, string email)
        {
            InitializeComponent();
            _reason = reason;
            _email = email;
            _apiService = new ApiService();
            UpdateUIForReason();
        }
        
        private void UpdateUIForReason()
        {
            EmailLabel.Text = _email;
            if (_reason == VerificationReason.EmailConfirmation)
            {
                TitleLabel.Text = "Підтвердження Пошти";
                InstructionsLabel.Text = "Введіть код, відправлений на пошту";
            }
            else
            {
                TitleLabel.Text = "Відновлення пароля";
                InstructionsLabel.Text = "Ви отримаєте код на пошту";
            }
        }
        private async Task SendVerificationCode()
        {
            // Можна показати індикатор завантаження
            bool success = await _apiService.SendVerificationEmailAsync(_email);
            // Можна сховати індикатор

            if (!success)
            {
                await DisplayAlert("Помилка", "Не вдалося відправити код підтвердження. Спробуйте ще раз.", "OK");
            }
            else
            {
                // Можна показати повідомлення типу "Код відправлено"
                Debug.WriteLine("Код підтвердження відправлено на " + _email);
            }
        }

        
        private void OnCodeEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry == null) return;

            // Якщо ввели символ, переходимо вперед
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                if (entry == CodeEntry1) CodeEntry2.Focus();
                else if (entry == CodeEntry2) CodeEntry3.Focus();
                else if (entry == CodeEntry3) CodeEntry4.Focus();
                else if (entry == CodeEntry4) CodeEntry5.Focus();
                else if (entry == CodeEntry5) CodeEntry6.Focus();
                else if (entry == CodeEntry6)
                {
                    entry.Unfocus(); // Прибираємо клавіатуру
                    ProcessEnteredCode();
                }
            }
        }

        private async void ProcessEnteredCode()
        {
            string fullCode = $"{CodeEntry1.Text}{CodeEntry2.Text}{CodeEntry3.Text}{CodeEntry4.Text}{CodeEntry5.Text}{CodeEntry6.Text}";
            if (fullCode.Length == 6)
            {
                // Отримай токен, якщо потрібен:
                string token = await SecureStorage.GetAsync("auth_token");

                bool success = await _apiService.VerifyEmailCodeAsync(_email, fullCode, token);

                if (success)
                {
                    await DisplayAlert("Успіх", "Ваш e-mail підтверджено!", "OK");
                    // Перенаправити куди треба, наприклад:
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    await DisplayAlert("Помилка", "Невірний або прострочений код.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Увага", "Введіть повний 6-значний код.", "OK");
            }
        }



        private void OnResendCodeTapped(object sender, TappedEventArgs e)
        {
            // TODO: Повторна відправка коду
            DisplayAlert("Інфо", "Запит на повторну відправку коду", "OK");
        }
    }
}