namespace AutoMarket
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // ЗАМІНІТЬ ЦЕЙ БЛОК:
            // MainPage = new Login();

            // НА ЦЕЙ:
            MainPage = new NavigationPage(new Login());
        }
    }
}