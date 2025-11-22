using AutoMarket.ViewModels;


namespace AutoMarket;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService;
    private FileResult _newAvatarFileResult = null;
    private string _currentAvatarUrl = null;
    private string _currentUserEmail = null;
    public ProfilePage(ProfilePageViewModel viewModel)
	{
		InitializeComponent();
        _apiService = new ApiService();
        BindingContext = viewModel;
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
            FirstName.Text = profile.firstName;
            /*LastNameEntry.Text = profile.lastName;*/
            _currentAvatarUrl = profile.avatarUrl;
            if (!string.IsNullOrEmpty(_currentAvatarUrl))
            {

                Avatar.Source = _currentAvatarUrl;
            }
            else
            {

                Avatar.Source = "profile_icon.png";
            }

        }
    }
    private async void OnSettingsTapped(object sender, TappedEventArgs e)
    {
        // ВИКОНУЄМО АСИНХРОННИЙ ПЕРЕХІД
        // 'ProfileEdit' - це наш маршрут, який ми зареєструємо нижче.
        await Shell.Current.GoToAsync("ProfileEdit");
    }
    private async void ExitProfileButtonClicked(object sender, EventArgs e)
    {
        // 1. Запитуємо підтвердження (не обов'язково, але гарний тон)
        bool answer = await DisplayAlert("Вихід", "Ви дійсно хочете вийти з акаунту?", "Так", "Ні");
        if (!answer) return;

        // 2. Очищаємо ВСІ дані сесії
        SecureStorage.Remove("auth_token");
        SecureStorage.Remove("user_id");
        SecureStorage.Remove("user_email");

        // Якщо ти зберігав ще щось (наприклад, ім'я), видали і його:
        // SecureStorage.Remove("user_firstName");

        // 3. Перенаправляємо на сторінку Логіну
        // Важливо: використовуємо "//Login", щоб очистити стек навігації.
        // Користувач не зможе натиснути кнопку "Назад", щоб повернутися в профіль.
        Application.Current.MainPage = new NavigationPage(new Login());
    }


}