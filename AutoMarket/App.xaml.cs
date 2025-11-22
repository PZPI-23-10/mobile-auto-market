<<<<<<< Updated upstream
﻿namespace AutoMarket
=======
﻿using System.Globalization;
using AutoMarket.Services;

namespace AutoMarket
>>>>>>> Stashed changes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

<<<<<<< Updated upstream
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
=======
            // 1. Встановлюємо мову
            LocalizationManager.Instance.SetLanguage();

            // 2. !! ВАЖЛИВО !!
            // Встановлюємо ТИМЧАСОВУ сторінку, щоб додаток не падав,
            // поки ми перевіряємо токен. Просто крутилка посеред екрану.
            MainPage = new ContentPage
            {
                BackgroundColor = Colors.White,
                Content = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Colors.Black,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                }
            };

            // 3. Запускаємо перевірку.
            // Коли вона закінчиться, вона САМА замінить MainPage на потрібну сторінку.
            CheckLoginStatus();
        }

        private async void CheckLoginStatus()
        {
            // Отримуємо токен з безпечного сховища
            string token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                // ЯКЩО Є ТОКЕН -> Запускаємо головне меню
                MainPage = new AppShell();
            }
            else
            {
                // ЯКЩО НЕМАЄ -> Запускаємо сторінку входу
                // Ми огортаємо її в NavigationPage, щоб працювала навігація на SignUp
                MainPage = new NavigationPage(new Login());
            }
        }

        // Метод для виклику після успішного логіну
        public static void LoginSuccess()
        {
            // Перемикаємо на головне меню
            Current.MainPage = new AppShell();
        }

        // Метод для виходу (Logout)
        public static void Logout()
        {
            // Видаляємо токен
            SecureStorage.Remove("auth_token");
            // Перемикаємо на сторінку входу
            Current.MainPage = new NavigationPage(new Login());
        }
>>>>>>> Stashed changes
    }
}