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
            System.Diagnostics.Debug.WriteLine($"[ProcessEnteredCode] Зібраний код для відправки: '{fullCode}'");

            if (fullCode.Length != 6)
            {
                await DisplayAlert("Увага", "Введіть повний 6-значний код.", "OK");
                return;
            }

            // --- ✅ ПОЧАТОК НОВОЇ ЛОГІКИ ---

            if (_reason == VerificationReason.EmailConfirmation)
            {
                // === 1. ЛОГІКА ПІДТВЕРДЖЕННЯ ПОШТИ (Юзер залогінений) ===
                string token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Помилка", "Сесія не знайдена. Увійдіть знову.", "OK");
                    return;
                }

                // Викликаємо старий, правильний метод
                bool success = await _apiService.VerifyEmailCodeAsync(fullCode, token);

                if (success)
                {
                    await DisplayAlert("Успіх", "Ваш e-mail підтверджено!", "OK");
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    await DisplayAlert("Помилка", "Невірний або прострочений код.", "OK");
                    ClearCodeEntries(); // Очищуємо поля
                }
            }
            else if (_reason == VerificationReason.PasswordReset)
            {
                // === 2. ЛОГІКА СКИДАННЯ ПАРОЛЯ (Юзер НЕ залогінений) ===

                // Викликаємо новий метод API, який не потребує токена
                bool codeIsValid = await _apiService.ConfirmPasswordResetCodeAsync(_email, fullCode);

                if (codeIsValid)
                {
                    // УСПІХ! Код вірний.
                    // Тепер ми маємо перекинути юзера на НОВУ сторінку,
                    // де він введе НОВИЙ пароль.

                    await DisplayAlert("Код вірний!", "Тепер введіть новий пароль.", "OK");

                    // Тобі потрібно створити нову сторінку 'SetNewPasswordPage'
                    // і передати їй email, щоб вона знала, для кого міняти пароль.
                    // (Код, схоже, вже не потрібен для наступного API)
                    await Navigation.PushAsync(new PasswordReset(_email));
                }
                else
                {
                    // ПОМИЛКА: Код невірний
                    await DisplayAlert("Помилка", "Невірний або прострочений код.", "OK");
                    ClearCodeEntries(); // Очищуємо поля
                }
            }
        }

        // Додай цей допоміжний метод у свій клас, щоб очищувати поля
        private void ClearCodeEntries()
        {
            CodeEntry1.Text = "";
            CodeEntry2.Text = "";
            CodeEntry3.Text = "";
            CodeEntry4.Text = "";
            CodeEntry5.Text = "";
            CodeEntry6.Text = "";
            CodeEntry1.Focus();
        }
        /* private async void ProcessEnteredCode()
         {
             string fullCode = $"{CodeEntry1.Text}{CodeEntry2.Text}{CodeEntry3.Text}{CodeEntry4.Text}{CodeEntry5.Text}{CodeEntry6.Text}";

             // ДОДАЙ ЦЕЙ РЯДОК, щоб бачити, що саме ми відправляємо
             System.Diagnostics.Debug.WriteLine($"[ProcessEnteredCode] Зібраний код для відправки: '{fullCode}'");

             if (fullCode.Length == 6)
             {
                 string token = await SecureStorage.GetAsync("auth_token");

                 // Викликаємо оновлений метод
                 bool success = await _apiService.VerifyEmailCodeAsync(fullCode, token);

                 if (success)
                 {
                     await DisplayAlert("Успіх", "Ваш e-mail підтверджено!", "OK");
                     await Navigation.PopToRootAsync();
                 }
                 else
                 {
                     await DisplayAlert("Помилка", "Невірний або прострочений код.", "OK");
                     // Можливо, тут варто очистити поля вводу
                     CodeEntry1.Text = "";
                     CodeEntry2.Text = "";
                     CodeEntry3.Text = "";
                     CodeEntry4.Text = "";
                     CodeEntry5.Text = "";
                     CodeEntry6.Text = "";
                     CodeEntry1.Focus();
                 }
             }
             else
             {
                 await DisplayAlert("Увага", "Введіть повний 6-значний код.", "OK");
             }
         }*/



        private async void OnResendCodeTapped(object sender, TappedEventArgs e)
        {
            // 1. Блокуємо кнопку/лейбл, щоб уникнути повторних натискань
            if (sender is View resendView)
            {
                resendView.IsEnabled = false;
            }

            // 2. Викликаємо сервіс для відправки коду
            // Ми використовуємо '_email', який зберегли при відкритті сторінки
            bool success = await _apiService.SendVerificationEmailAsync(_email);

            // 3. Обробляємо результат
            if (success)
            {
                await DisplayAlert("Успіх", "Новий код відправлено на вашу пошту.", "OK");
            }
            else
            {
                await DisplayAlert("Помилка", "Не вдалося відправити код повторно. Спробуйте пізніше.", "OK");
            }

            // 4. (Опціонально) Робимо паузу перед тим, як знову ввімкнути кнопку
            await Task.Delay(10000); // 10-секундний кулдаун

            if (sender is View resendViewAfterDelay)
            {
                resendViewAfterDelay.IsEnabled = true;
            }
        }
    }
}