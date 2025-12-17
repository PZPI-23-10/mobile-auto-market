namespace AutoMarket;

using AutoMarket.Models;
using System.Text.RegularExpressions;

public partial class SignUp : ContentPage
{
    private readonly ApiService _apiService;

    public SignUp()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    // Візуальна зміна кольорів при вводі (можеш додати сюди логіку для спец. символу, якщо в XAML є 4-та галочка)
    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        var password = e.NewTextValue ?? "";

        var validColor = Colors.White;
        var invalidColor = Colors.Gray;

        // 1. Довжина (8 символів)
        bool isLengthValid = password.Length >= 8;
        LengthRequirementLabel.TextColor = isLengthValid ? validColor : invalidColor;
        LengthCheckmark.Opacity = isLengthValid ? 1.0 : 0.5;

        // 2. Цифра
        bool hasDigit = Regex.IsMatch(password, @"\d");
        DigitRequirementLabel.TextColor = hasDigit ? validColor : invalidColor;
        DigitCheckmark.Opacity = hasDigit ? 1.0 : 0.5;

        // 3. Великі та малі літери
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        LettersRequirementLabel.TextColor = (hasLower && hasUpper) ? validColor : invalidColor;
        LettersCheckmark.Opacity = (hasLower && hasUpper) ? 1.0 : 0.5;

        // 4. ✅ СПЕЦІАЛЬНИЙ ЗНАК (Додаємо логіку сюди)
        // Перевіряє наявність будь-якого символу, що НЕ є буквою і НЕ є цифрою
        bool hasSpecial = Regex.IsMatch(password, @"[^a-zA-Z0-9]");
        SpecialRequirementLabel.TextColor = hasSpecial ? validColor : invalidColor;
        SpecialCheckmark.Opacity = hasSpecial ? 1.0 : 0.5;
    }

    private void OnPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // 1. Блокуємо кнопку, щоб не натиснули двічі
        RegisterButton.IsEnabled = false;

        try
        {
            // 2. Перевірка пустих полів
            if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
                string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
                string.IsNullOrWhiteSpace(PhoneNumberEntry.Text) ||
                string.IsNullOrWhiteSpace(CountryEntry.Text) ||
                string.IsNullOrWhiteSpace(AddressEntry.Text))
            {
                await DisplayAlert("Помилка", "Будь ласка, заповніть усі обов'язкові поля", "OK");
                return;
            }

            // 3. Перевірка пошти
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(EmailEntry.Text, emailPattern))
            {
                await DisplayAlert("Помилка", "Введіть коректну адресу пошти", "OK");
                return;
            }

            // 4. ✅ ЖОРСТКА ВАЛІДАЦІЯ ПАРОЛЯ (+ Спец. символ)
            string password = PasswordEntry.Text;

            // Перевіряємо наявність спец. символу (все, що не буква і не цифра)
            // Або можна Regex: [^a-zA-Z0-9]
            bool hasSpecialChar = Regex.IsMatch(password, @"[^a-zA-Z0-9]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasUpper = Regex.IsMatch(password, "[A-Z]");
            bool hasLower = Regex.IsMatch(password, "[a-z]");
            bool hasLength = password.Length >= 8;

            if (!hasLength || !hasDigit || !hasUpper || !hasLower || !hasSpecialChar)
            {
                await DisplayAlert("Слабкий пароль",
                    "Пароль має містити:\n" +
                    "- Мінімум 6 символів\n" +
                    "- Велику літеру (A-Z)\n" +
                    "- Малу літеру (a-z)\n" +
                    "- Цифру (0-9)\n" +
                    "- Спеціальний символ (! @ # - _ тощо)",
                    "OK");
                return;
            }

            // 5. Формуємо запит
            var registerRequest = new RegisterRequest
            {
                firstName = FirstNameEntry.Text,
                lastName = LastNameEntry.Text,
                email = EmailEntry.Text,
                password = PasswordEntry.Text,
                phoneNumber = PhoneNumberEntry.Text,
                dateOfBirth = DateOfBirthPicker.Date,
                country = CountryEntry.Text,
                address = AddressEntry.Text,
                aboutUrself = aboutUrselfEditor.Text
            };

            // 6. Відправляємо запит
            string errorResult = await _apiService.RegisterAsync(registerRequest);

            if (errorResult == null)
            {
                // ✅ УСПІХ!
                await DisplayAlert("Успіх!", "Реєстрація пройшла успішно. Тепер увійдіть у свій акаунт.", "OK");

                // Повертаємося назад на сторінку Логіна
                if (Navigation.NavigationStack.Count > 0)
                {
                    await Navigation.PopAsync();
                }
                else
                {
                    // Якщо раптом стек пустий (рідкість), форсуємо перехід
                    Application.Current.MainPage = new NavigationPage(new MailLogin());
                }
            }
            else
            {
                // ПОМИЛКА!
                await DisplayAlert("Помилка реєстрації", errorResult, "OK");
            }
        }
        finally
        {
            // 7. Розблоковуємо кнопку в будь-якому випадку (навіть якщо вилетіла помилка)
            RegisterButton.IsEnabled = true;
        }
    }

    private void OnTermsTapped(object sender, TappedEventArgs e)
    {
        DisplayAlert("Навігація", "Перехід до Умов надання послуг", "OK");
    }

    private void OnPrivacyPolicyTapped(object sender, TappedEventArgs e)
    {
        DisplayAlert("Навігація", "Перехід до Політики приватності", "OK");
    }
}