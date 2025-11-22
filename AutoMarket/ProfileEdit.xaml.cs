using AutoMarket.Models;
using System.Diagnostics;

namespace AutoMarket;

public partial class ProfileEdit : ContentPage
{
    private readonly ApiService _apiService;
    private FileResult _newAvatarFileResult = null; 
    private string _currentAvatarUrl = null;
    private string _currentUserEmail = null; 

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
            if (profile.dateOfBirth.HasValue)
            {
                // Якщо дата є, встановлюємо її
                DateOfBirthPicker.Date = profile.dateOfBirth.Value.ToLocalTime();
            }
            else
            {
                // Якщо дати немає (null), ставимо якусь дату за замовчуванням
                // Наприклад, 18 років тому
                DateOfBirthPicker.Date = DateTime.Now.AddYears(-18);
            }
            CountryEntry.Text = profile.country;
            AddressEntry.Text = profile.address;
            AboutYourselfEditor.Text = profile.aboutYourself;

            _currentAvatarUrl = profile.avatarUrl;
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

            _newAvatarFileResult = result; // <-- НОВИЙ КОД
            AvatarImageButton.Source = ImageSource.FromFile(result.FullPath); // <-- НОВИЙ КОD
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

        // --- ПОЧАТОК НОВОГО БЛОКУ: Валідація полів ---

        // 2. Перевіряємо, чи заповнені обов'язкові поля
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text))
        {
            await DisplayAlert("Порожнє поле", "Будь ласка, введіть ваше ім'я.", "OK");
            ChangeInfoButton.IsEnabled = true; // Вмикаємо кнопку назад
            return; // Зупиняємо виконання
        }

        if (string.IsNullOrWhiteSpace(LastNameEntry.Text))
        {
            await DisplayAlert("Порожнє поле", "Будь ласка, введіть ваше прізвище.", "OK");
            ChangeInfoButton.IsEnabled = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneNumberEntry.Text))
        {
            await DisplayAlert("Порожнє поле", "Будь ласка, введіть ваш номер телефону.", "OK");
            ChangeInfoButton.IsEnabled = true;
            return;
        }
        if (string.IsNullOrWhiteSpace(CountryEntry.Text))
        {
            await DisplayAlert("Порожнє поле", "Будь ласка, введіть вашу країну.", "OK");
            ChangeInfoButton.IsEnabled = true;
            return;
        }
        if (string.IsNullOrWhiteSpace(AddressEntry.Text))
        {
            await DisplayAlert("Порожнє поле", "Будь ласка, введіть вашу адресу.", "OK");
            ChangeInfoButton.IsEnabled = true;
            return;
        }

        // (Можеш додати сюди інші перевірки, наприклад, на 'Country' або 'Address', якщо вони обов'язкові)

        // --- КІНЕЦЬ НОВОГО БЛОКУ ---


        // --- ПОЧАТОК ТВОЄЇ ЛОГІКИ API ---

        Stream photoStream = null;
        string photoFileName = null;
        string serverResponse = null;

        try
        {
            // 3. Збираємо дані з полів (тепер ми знаємо, що вони не порожні)
            var profileData = new EditProfileRequest
            {
                firstName = FirstNameEntry.Text.Trim(), // .Trim() прибирає зайві пробіли
                lastName = LastNameEntry.Text.Trim(),
                phoneNumber = PhoneNumberEntry.Text.Trim(),
                dateOfBirth = DateOfBirthPicker.Date.ToUniversalTime(),
                country = CountryEntry.Text.Trim(),
                address = AddressEntry.Text.Trim(),
                aboutYourself = AboutYourselfEditor.Text?.Trim() ?? "",
            };

            // 4. Готуємо файл, ЯКЩО він був обраний
            if (_newAvatarFileResult != null)
            {
                photoStream = await _newAvatarFileResult.OpenReadAsync();
                photoFileName = _newAvatarFileResult.FileName;
            }

            // 5. Викликаємо НОВИЙ метод ApiService
            serverResponse = await _apiService.UpdateUserProfileAsync(
                profileData,
                photoStream,
                photoFileName,
                token
            );
        }
        catch (Exception ex)
        {
            // Ловимо будь-які помилки під час підготовки або відправки
            Debug.WriteLine($"[OnSaveClicked] Critical Error: {ex.Message}");
            serverResponse = $"Критична помилка: {ex.Message}";
        }
        finally
        {
            // 6. ДУЖЕ ВАЖЛИВО: закриваємо стрім файлу, щоб звільнити пам'ять
            photoStream?.Close();
        }

        // --- КІНЕЦЬ ЛОГІКИ API ---

        // 7. Обробляємо відповідь сервера
        System.Diagnostics.Debug.WriteLine($"Відповідь сервера: '{serverResponse}'");

        bool isSuccess = serverResponse == "OK" ||
                         (serverResponse != null &&
                          !serverResponse.Contains("Помилка", StringComparison.OrdinalIgnoreCase) &&
                          !serverResponse.Contains("Статус:", StringComparison.OrdinalIgnoreCase));

        if (isSuccess)
        {
            await DisplayAlert("Успіх", "Профіль оновлено.", "OK");

            _newAvatarFileResult = null; // Скидаємо вибраний файл
            await LoadUserProfile(); // Оновлюємо дані на екрані з сервера
            await Shell.Current.GoToAsync($"//{nameof(ProfilePage)}");
        }
        else
        {
            // Показуємо помилку, яку повернув сервер
            await DisplayAlert("Помилка оновлення", serverResponse ?? "Невідома помилка", "OK");
        }

        // 8. Вмикаємо кнопку назад
        ChangeInfoButton.IsEnabled = true;
    }
    private async void OnVerifyEmailClicked(object sender, EventArgs e)
    {
        // НЕПРАВИЛЬНО (бере старі дані):
        // string userEmail = await SecureStorage.GetAsync("user_email"); 

        // ПРАВИЛЬНО (бере актуальні дані, завантажені щойно):
        string userEmail = _currentUserEmail;

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            await DisplayAlert("Помилка", "Не вдалося отримати ваш Email з профілю. Спробуйте перезавантажити сторінку.", "OK");
            return;
        }

        bool success = await _apiService.SendVerificationEmailAsync(userEmail);

        if (success)
        {
            // Тепер 'userEmail' - це АКТУАЛЬНА пошта
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
    





}