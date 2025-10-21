using Microsoft.Maui.Controls;

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

        public ConfirmationPage(VerificationReason reason, string email)
        {
            InitializeComponent();
            _reason = reason;
            _email = email;
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

        // ✅ НОВА, ПРОСТА ЛОГІКА ПЕРЕКЛЮЧЕННЯ
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

        private void ProcessEnteredCode()
        {
            string fullCode = $"{CodeEntry1.Text}{CodeEntry2.Text}{CodeEntry3.Text}{CodeEntry4.Text}{CodeEntry5.Text}{CodeEntry6.Text}";
            if (fullCode.Length == 6)
            {
                // TODO: Перевірка коду на сервері
                DisplayAlert("Інфо", $"Введено код: {fullCode}", "OK");
            }
        }

        private void OnResendCodeTapped(object sender, TappedEventArgs e)
        {
            // TODO: Повторна відправка коду
            DisplayAlert("Інфо", "Запит на повторну відправку коду", "OK");
        }
    }
}