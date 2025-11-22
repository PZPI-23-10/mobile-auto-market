namespace AutoMarket
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
<<<<<<< Updated upstream
            return new Window(new NavigationPage(new Login()));
=======
            // Налаштовуємо мову (з твого нижнього блоку)
            LocalizationManager.Instance.SetLanguage();

            // Повертаємо вікно (тут вибирай одне з двох):

            // Варіант А (якщо використовуєш AppShell і перевірку логіна робиш всередині нього):
            return new Window(new AppShell());

            // АБО Варіант Б (якщо хочеш жорстко відкрити Логін):
            // return new Window(new NavigationPage(new Login()));
>>>>>>> Stashed changes
        }
    }

    
}