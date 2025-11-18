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
    private void OnPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        var password = e.NewTextValue ?? ""; 

      
        var validColor = Colors.White; 
        var invalidColor = Colors.Gray;  

       
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

    
    private void OnPasswordVisibilityToggleClicked(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // 1. Валідація: перевіряємо, чи всі основні поля заповнені
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            /*await DisplayAlert("Помилка", "Будь ласка, заповніть усі обов'язкові поля", "OK");
            return;*/
        }

        // (Твоя валідація пошти залишається тут)
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(EmailEntry.Text, emailPattern))
        {
            await DisplayAlert("Помилка", "Введіть коректну адресу пошти", "OK");
            return;
        }

        
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
            aboutYourself = AboutYourselfEditor.Text
        };

     
        RegisterButton.IsEnabled = true;

       
        string errorResult = await _apiService.RegisterAsync(registerRequest);

      
        if (errorResult == null)
        {
           
            await DisplayAlert("Успіх!", "Реєстрація пройшла успішно.", "OK");
            await Navigation.PushAsync(new MailLogin());
        }
        else
        {
           
            await DisplayAlert("Помилка реєстрації", errorResult, "OK");
        }

       
        RegisterButton.IsEnabled = true;
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