using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using AutoMarket.ViewModels;
using AutoMarket.ViewModel;
using AutoMarket.Views;// !! Додайте це, щоб MAUI знав про CarListing

namespace AutoMarket
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
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
            builder.Services.AddTransient<CarDetailsViewModel>();
            builder.Services.AddTransient<CarDetailsPage>();
            builder.Services.AddTransient<FavoritesPage>();
            builder.Services.AddTransient<FavoritesViewModel>();
            builder.Services.AddTransient<MyListingsPage>();
            builder.Services.AddTransient<MyListingsViewModel>();
            builder.Services.AddTransient<ChatsViewModel>();
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ConversationViewModel>();
            builder.Services.AddTransient<ConversationPage>();
            builder.Services.AddTransient<FullMapPage>();
            builder.Services.AddSingleton<ChatHubService>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
