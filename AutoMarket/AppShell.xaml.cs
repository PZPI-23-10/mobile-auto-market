namespace AutoMarket
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("ProfileEdit", typeof(ProfileEdit));

        }

    }
}
