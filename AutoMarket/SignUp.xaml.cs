namespace AutoMarket;

using System.Text.RegularExpressions;

public partial class SignUp : ContentPage
{
	public SignUp()
	{
		InitializeComponent();
	}
    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        var password = e.NewTextValue ?? ""; // Отримуємо поточний текст пароля

        // Кольори для станів
        var validColor = Colors.White; // Яскравий колір
        var invalidColor = Colors.Gray;  // Тусклий колір

        // --- Перевірка правила 1: Довжина ---
        bool isLengthValid = password.Length >= 6;
        LengthRequirementLabel.TextColor = isLengthValid ? validColor : invalidColor;
        LengthCheckmark.Opacity = isLengthValid ? 1.0 : 0.5;

        // --- Перевірка правила 2: Наявність цифри ---
        bool hasDigit = Regex.IsMatch(password, @"\d");
        DigitRequirementLabel.TextColor = hasDigit ? validColor : invalidColor;
        DigitCheckmark.Opacity = hasDigit ? 1.0 : 0.5;

        // --- Перевірка правила 3: Великі та малі літери ---
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        LettersRequirementLabel.TextColor = (hasLower && hasUpper) ? validColor : invalidColor;
        LettersCheckmark.Opacity = (hasLower && hasUpper) ? 1.0 : 0.5;
    }

    private void OnTermsSwitchToggled(object sender, ToggledEventArgs e)
    {
       
        bool isToggled = e.Value;

        if (isToggled)
        {
            
            RegisterButton.IsEnabled = true;
            RegisterButton.Opacity = 1.0;
        }
        else
        {
            
            RegisterButton.IsEnabled = false;
            RegisterButton.Opacity = 0.5;
        }
    }
    private void OnPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
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


        await Navigation.PushAsync(new ConfirmationPage(VerificationReason.EmailConfirmation, userEmail));
    }

    private void OnTermsTapped(object sender, TappedEventArgs e)
    {
        // TODO: Відкрити посилання на Угоду про надання послуг
        DisplayAlert("Навігація", "Перехід до Умов надання послуг", "OK");
    }

    private void OnPrivacyPolicyTapped(object sender, TappedEventArgs e)
    {
        // TODO: Відкрити посилання на Політику приватності
        DisplayAlert("Навігація", "Перехід до Політики приватності", "OK");
    }
}