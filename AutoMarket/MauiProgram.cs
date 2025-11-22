using Microsoft.Extensions.Logging;
<<<<<<< Updated upstream
=======
using CommunityToolkit.Maui;
using AutoMarket.ViewModels;
using AutoMarket.ViewModel; // !! Додайте це, щоб MAUI знав про CarListing
>>>>>>> Stashed changes

namespace AutoMarket
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
<<<<<<< Updated upstream

=======
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ProfilePageViewModel>();
            builder.Services.AddTransient<AddListingViewModel>();
            builder.Services.AddTransient<Login>();
            builder.Services.AddTransient<SignUp>();
            builder.Services.AddTransient<MailLogin>();
            builder.Services.AddTransient<ProfileEdit>();
>>>>>>> Stashed changes
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
