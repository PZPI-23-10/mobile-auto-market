using AutoMarket.ViewModels; // Your ViewModel namespace
using AutoMarket.Views;      // For MyListingsPage
using AutoMarket.Services;   // For ApiService

namespace AutoMarket.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ApiService _apiService;
        private readonly IServiceProvider _services; // Service provider for navigation

        private string _currentAvatarUrl = null;

        // Constructor
        public ProfilePage(ProfilePageViewModel viewModel, IServiceProvider services)
        {
            InitializeComponent();

            _apiService = new ApiService();
            BindingContext = viewModel;
            _services = services;
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
                // await DisplayAlert("Помилка завантаження профілю", error, "OK");
            }
            else if (profile != null)
            {
                if (FirstName != null) FirstName.Text = profile.firstName;

                _currentAvatarUrl = profile.avatarUrl;

                if (Avatar != null)
                {
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
        }

        private async void OnSettingsTapped(object sender, TappedEventArgs e)
        {
            // Settings via Shell
            await Shell.Current.GoToAsync("ProfileEdit");
        }

        // Direct Navigation Logic
        private async void OnMyListingsTapped(object sender, TappedEventArgs e)
        {
            try
            {
                // 1. Спробуємо отримати сторінку через сервіси
                // Використовуємо GetService, щоб не впало, якщо сервіс не знайдено
                var myListingPage = _services.GetService(typeof(MyListingsPage)) as MyListingsPage;

                // 2. Перевіряємо, чи сторінка створилась
                if (myListingPage == null)
                {
                    await DisplayAlert("Помилка", "Не вдалося створити сторінку MyListingsPage. Перевірте MauiProgram.cs", "OK");
                    return;
                }

                // 3. Переходимо
                await Navigation.PushAsync(myListingPage);
            }
            catch (Exception ex)
            {
                // 4. ЯКЩО ЩОСЬ ВПАЛО - ПОКАЖИ МЕНІ ЦЕ!
                // Виводимо повний текст помилки на екран
                await DisplayAlert("CRASH INFO", ex.ToString(), "OK");
            }
        }

        private async void ExitProfileButtonClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Вихід", "Ви дійсно хочете вийти з акаунту?", "Так", "Ні");
            if (!answer) return;

            SecureStorage.Remove("auth_token");
            SecureStorage.Remove("user_id");
            SecureStorage.Remove("user_email");

            Application.Current.MainPage = new NavigationPage(new Login());
        }
    }
}