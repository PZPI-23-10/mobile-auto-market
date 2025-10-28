using AutoMarket.Models;
using System.Diagnostics;

namespace AutoMarket;

public partial class ProfileEdit : ContentPage
{
    private readonly ApiService _apiService;
    private string _newAvatarFilePath = null;
    private string _currentAvatarUrl = null;

    public ProfileEdit()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUserProfile();
    }

    private async Task LoadUserProfile()
    {
        string userId = await SecureStorage.GetAsync("user_id");
        string token = await SecureStorage.GetAsync("auth_token");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            await DisplayAlert("Помилка", "Сесія не знайдена. Увійдіть знову.", "OK");

            return;
        }

        var (profile, error) = await _apiService.GetUserProfileAsync(userId, token);

        if (error != null)
        {
            await DisplayAlert("Помилка завантаження профілю", error, "OK");


        }
        else if (profile != null)
        {
            FirstNameEntry.Text = profile.firstName;
            LastNameEntry.Text = profile.lastName;
            PhoneNumberEntry.Text = profile.phoneNumber;
            DateOfBirthPicker.Date = profile.dateOfBirth.ToLocalTime();
            CountryEntry.Text = profile.country;
            AddressEntry.Text = profile.address;
            AboutYourselfEditor.Text = profile.aboutYourself;

            _currentAvatarUrl = profile.urlPhoto;
            if (!string.IsNullOrEmpty(_currentAvatarUrl))
            {

                AvatarImageButton.Source = _currentAvatarUrl;
            }
            else
            {

                AvatarImageButton.Source = "profile_icon.png";
            }
            if (profile.isVerified)
            {
                VerifyEmailButton.IsVisible = false;

            }
            else
            {
                VerifyEmailButton.IsVisible = true;

            }


        }
        else
        {

            await DisplayAlert("Помилка", "Не вдалося завантажити профіль (невідома причина).", "OK");
        }
    }


    private async void OnAvatarClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Виберіть аватар"
            });

            if (result == null) return;

            _newAvatarFilePath = result.FullPath;
            AvatarImageButton.Source = ImageSource.FromFile(_newAvatarFilePath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося вибрати фото: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        
        ChangeInfoButton.IsEnabled = false; 

        // 1. Перевіряємо токен сесії
        string token = await SecureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token))
        {
            await DisplayAlert("Помилка", "Сесія не знайдена. Будь ласка, увійдіть знову.", "OK");
            ChangeInfoButton.IsEnabled = true; 
                                               
            return;
        }

        string finalAvatarUrl = _currentAvatarUrl; 
        string uploadError = null; 

      
        if (_newAvatarFilePath != null)
        {
            

            var (newUrl, error) = await _apiService.UploadToCloudinaryAsync(_newAvatarFilePath);

            

            if (error != null)
            {
              
                uploadError = error;
            }
            else
            {
           
                finalAvatarUrl = newUrl;
                _currentAvatarUrl = finalAvatarUrl; 
                _newAvatarFilePath = null; 
            }
        }

        // 3. Якщо була помилка завантаження аватара - показуємо її і виходимо
        if (uploadError != null)
        {
            await DisplayAlert("Помилка завантаження аватара", uploadError, "OK");
            ChangeInfoButton.IsEnabled = true; // Розблоковуємо кнопку
            return;
        }

        // 4. Збираємо дані з усіх полів форми
        var profileData = new EditProfileRequest
        {
            password = "string",
            firstName = FirstNameEntry.Text,
            lastName = LastNameEntry.Text,
            phoneNumber = PhoneNumberEntry.Text,
            dateOfBirth = DateOfBirthPicker.Date.ToUniversalTime(), 
            country = CountryEntry.Text,
            address = AddressEntry.Text,
            aboutYourself = AboutYourselfEditor.Text,
            urlPhoto = finalAvatarUrl 
        };

        // Логуємо дані, які відправляємо (для дебагу)
        System.Diagnostics.Debug.WriteLine($"Відправляємо дані: {System.Text.Json.JsonSerializer.Serialize(profileData)}");

        // 5. Викликаємо ApiService для відправки даних на наш бекенд
        string serverResponse = await _apiService.UpdateUserProfileAsync(profileData, token);

        // Логуємо відповідь сервера (для дебагу)
        System.Diagnostics.Debug.WriteLine($"Відповідь сервера: '{serverResponse}'");

        // 6. Показуємо відповідь сервера користувачу
        await DisplayAlert("Відповідь сервера", serverResponse ?? "Сервер не відповів", "OK");

        // 7. Оновлюємо дані на екрані ТІЛЬКИ якщо не було явної помилки
        // (Цю перевірку можна зробити точнішою, якщо знати формат успішної відповіді)
        bool updateSeemsSuccessful = serverResponse != null &&
                                    !serverResponse.Contains("Помилка", StringComparison.OrdinalIgnoreCase) &&
                                    !serverResponse.Contains("Статус:", StringComparison.OrdinalIgnoreCase) &&
                                    !serverResponse.Contains("Error", StringComparison.OrdinalIgnoreCase);

        if (updateSeemsSuccessful)
        {
            await LoadUserProfile(); 
        }
        
        ChangeInfoButton.IsEnabled = true;
    }
    private async void OnVerifyEmailClicked(object sender, EventArgs e)
    {
     
        string userEmail = await SecureStorage.GetAsync("user_email");

     
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await DisplayAlert("Помилка", "Не вдалося отримати ваш Email зі сховища. Спробуйте увійти знову.", "OK");
            return; 
        }

        
        bool success = await _apiService.SendVerificationEmailAsync(userEmail);

        if (success)
        {
            
            await Navigation.PushAsync(new ConfirmationPage(VerificationReason.EmailConfirmation, userEmail));
        }
        else
        {
            
            await DisplayAlert("Помилка", "Не вдалося відправити код підтвердження. Спробуйте ще раз.", "OK");
        }
    }

    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChangePassword());
    }
    /* private async void OnSaveClicked(object sender, EventArgs e)
     {
         string token = await SecureStorage.GetAsync("auth_token");
         if (string.IsNullOrEmpty(token))
         {
             await DisplayAlert("Помилка", "Сесія не знайдена.", "OK");
             return;
         }

         string finalAvatarUrl = _currentAvatarUrl; 
         string uploadError = null; 


         if (_newAvatarFilePath != null)
         {

             var (newUrl, error) = await _apiService.UploadToCloudinaryAsync(_newAvatarFilePath);


             if (error != null)
             {

                 uploadError = error;
             }
             else
             {

                 finalAvatarUrl = newUrl;
                 _currentAvatarUrl = finalAvatarUrl;
                 _newAvatarFilePath = null;
             }
         }


         if (uploadError != null)
         {
             await DisplayAlert("Помилка завантаження аватара", uploadError, "OK");
             return; 
         }
         var profileData = new EditProfileRequest
         {
             firstName = FirstNameEntry.Text,
             lastName = LastNameEntry.Text,
             phoneNumber = PhoneNumberEntry.Text,
             *//*password = string.IsNullOrWhiteSpace(PasswordEntry.Text) ? null : PasswordEntry.Text,*//*
             dateOfBirth = DateOfBirthPicker.Date.ToUniversalTime(), 
             country = CountryEntry.Text,
             address = AddressEntry.Text,
             aboutYourself = AboutYourselfEditor.Text,
             urlPhoto = finalAvatarUrl 
         };
         System.Diagnostics.Debug.WriteLine($"Відправляємо дані: {System.Text.Json.JsonSerializer.Serialize(profileData)}");
         string updateError = await _apiService.UpdateUserProfileAsync(profileData, token);
         System.Diagnostics.Debug.WriteLine($"updateError: '{updateError}'");
         if (updateError == null)
         {
             // УСПІХ!
             await DisplayAlert("Успіх!", "Дані профілю оновлено.", "OK");
             await LoadUserProfile(); 
         }
         else
         {
             // ПОМИЛКА! Показуємо те, що повернув сервер
             await DisplayAlert("Помилка оновлення", updateError, "OK");
         }
     }*/





}