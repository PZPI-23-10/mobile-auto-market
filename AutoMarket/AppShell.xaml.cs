namespace AutoMarket;

public partial class AppShell : Shell
{
    public AppShell()
    {
<<<<<<< Updated upstream
        public AppShell()
        {
            InitializeComponent();
           
        }
=======
        InitializeComponent();

        // ❌ ВИДАЛИ РЯДКИ З Login, MainPage, FavoritesPage і т.д.,
        // ЯКЩО ВОНИ ВЖЕ Є В AppShell.xaml (в табах або як ShellContent).

        // 👇 Залиш ТІЛЬКИ ті сторінки, яких НЕМАЄ в нижньому меню/табах,
        // і на які ти переходиш через PushAsync (наприклад, деталі, чат, реєстрація)

        Routing.RegisterRoute(nameof(SignUp), typeof(SignUp));
        Routing.RegisterRoute(nameof(MailLogin), typeof(MailLogin));

        // MainPage, ProfilePage, FavoritesPage, Login - ВИДАЛЯЄМО звідси, 
        // бо вони вже прописані в XAML файлі!

        // А ці залишаємо, бо це детальні сторінки:
        Routing.RegisterRoute(nameof(ProfileEdit), typeof(ProfileEdit));
        Routing.RegisterRoute(nameof(AddListingPage), typeof(AddListingPage)); // Якщо вона не в табі
        Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage)); // Якщо вона не в табі
        Routing.RegisterRoute(nameof(ConfirmationPage), typeof(ConfirmationPage));

        // 🔥 ЗАПУСКАЄМО ПЕРЕВІРКУ
        CheckLoginStatus();
>>>>>>> Stashed changes
    }

    private async void CheckLoginStatus()
    {
        await Task.Delay(100);
        string token = await SecureStorage.GetAsync("auth_token");

        if (string.IsNullOrEmpty(token))
        {
            // Тепер це спрацює, бо Login більше не "Global Route", 
            // а повноцінна частина Shell завдяки XAML
            await GoToAsync($"//{nameof(Login)}");
        }
    }
}