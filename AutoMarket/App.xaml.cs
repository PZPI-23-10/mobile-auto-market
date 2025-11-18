using System.Globalization;
using AutoMarket.Services;


namespace AutoMarket
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

<<<<<<< Updated upstream
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
=======
            // ЗАМІНІТЬ ЦЕЙ БЛОК:
            // MainPage = new Login();
            LocalizationManager.Instance.SetLanguage();
            // НА ЦЕЙ:
            MainPage = new NavigationPage(new Login());
>>>>>>> Stashed changes
        }

    }
}