using AutoMarket.Models;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    // Реалізуємо IQueryAttributable, щоб отримати об'єкт машини при навігації
    public class CarDetailsViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private CarListing _listing;
        public CarListing Listing
        {
            get => _listing;
            set
            {
                _listing = value;
                OnPropertyChanged();
            }
        }

        // Команди
        public ICommand WriteCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand CallCommand { get; }
        public ICommand OpenFullMapCommand { get; }
        public CarDetailsViewModel()
        {
           
        BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
            CallCommand = new Command(OnCall);
            WriteCommand = new Command(OnWriteMessage);
            OpenFullMapCommand = new Command(async () => await OpenFullMap());
           
        }
       
        public async Task OpenFullMap()
        {
            if (Listing?.Latitude == null || Listing?.Longitude == null) return;

            // 1. Формуємо параметри (як ти робив з "Listing")
            var navigationParameter = new Dictionary<string, object>
    {
        { "lat", Listing.Latitude.Value },
        { "lng", Listing.Longitude.Value }
    };

            // 2. Викликаємо перехід (це додасть сторінку в стек і сама з'явиться стрілка)
            await Shell.Current.GoToAsync("FullMapPage", navigationParameter);
        }

        private CarCheckInfo _checkInfo;
        public CarCheckInfo CheckInfo
        {
            get => _checkInfo;
            set { _checkInfo = value; OnPropertyChanged(); }
        }

        private bool _isChecking;
        public bool IsChecking
        {
            get => _isChecking;
            set { _isChecking = value; OnPropertyChanged(); }
        }

        // Оновлюємо метод ApplyQueryAttributes
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("Listing") && query["Listing"] is CarListing car)
            {
                Listing = car;
                // ЗАПУСКАЄМО ПЕРЕВІРКУ АВТОМАТИЧНО
                LoadCarCheck();
            }
        }

        private async void LoadCarCheck()
        {
            if (string.IsNullOrEmpty(Listing.Number)) return;

            IsChecking = true;
            // Затримка, щоб UI встиг намалюватися
            await Task.Delay(500);

            var result = await new ApiService().CheckCarByNumberAsync(Listing.Number);

            if (result != null)
            {
                CheckInfo = result;
            }
            IsChecking = false;
        }
        private async void OnWriteMessage()
        {
            if (Listing == null) return;

            // 1. Перевіряємо, чи ми залогінені
            string token = await SecureStorage.GetAsync("auth_token");
            string myIdStr = await SecureStorage.GetAsync("user_id");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(myIdStr))
            {
                await App.Current.MainPage.DisplayAlert("Увага", "Щоб написати продавцю, потрібно увійти в акаунт.", "OK");
                // Тут можна перекинути на логін, якщо хочеш
                return;
            }

            if (Listing.UserId.ToString() == myIdStr)
            {
                await App.Current.MainPage.DisplayAlert("Ой", "Ви не можете писати самі собі!", "OK");
                return;
            }

            // 3. Створюємо чат через API
            var apiService = new ApiService();
            var chat = await apiService.GetOrCreateChatAsync(Listing.UserId, token);
            if (chat == null)
            {
                await Task.Delay(500); // Даємо серверу час "продуплитись"
                chat = await apiService.GetOrCreateChatAsync(Listing.UserId, token);
            }
            if (chat != null)
            {
                // 🔥 ОСЬ ТУТ МАГІЯ ПЕРЕМИКАННЯ ВКЛАДОК 🔥

                // Крок А: Примусово перемикаємось на вкладку "ChatPage" (це назва твого Route в AppShell)
                await Shell.Current.GoToAsync("///ChatPage");

                // Крок Б: Тепер, коли ми вже на вкладці чатів, відкриваємо діалог
                await Shell.Current.GoToAsync("ConversationPage", new Dictionary<string, object>
                {
                    { "CurrentChat", chat }
                });
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Помилка", "Не вдалося відкрити чат.", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private void OnCall()
        {
            // ТИМЧАСОВО: Дзвонимо на тестовий номер, бо в API поки немає телефону власника
            string phoneNumber = "0971234567";

            try
            {
                if (PhoneDialer.Default.IsSupported)
                    PhoneDialer.Default.Open(phoneNumber);
            }
            catch (Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Помилка", "Не вдалося здійснити дзвінок", "OK");
            }
        }
    }
}