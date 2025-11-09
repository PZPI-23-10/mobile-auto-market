using System.Diagnostics;
using Microsoft.Maui.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AutoMarket.Models;

namespace AutoMarket;

public partial class Login : ContentPage
{
    private readonly ApiService _apiService;
    public Login()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }
    private async void OnGoogleLoginTapped(object sender, TappedEventArgs e)
    {
        try
        {

            string clientId = "186738573523-s0guvbfia4q8tfjlvn3l893svgr4bemh.apps.googleusercontent.com";
            string redirectUri = "com.companyname.automarket:/auth";
            string codeVerifier = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "-").Replace("/", "").Replace("=", "");
            using var sha256 = SHA256.Create();
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            string codeChallenge = Convert.ToBase64String(challengeBytes)
                                     .Replace('+', '-')
                                     .Replace('/', '_')
                                     .TrimEnd('=');

            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri("https://accounts.google.com/o/oauth2/v2/auth" +
                        $"?client_id={clientId}" +
                        $"&redirect_uri={redirectUri}" +
                        "&response_type=code" +
                        "&scope=openid%20email%20profile" +
                        $"&code_challenge={codeChallenge}" +
                        "&code_challenge_method=S256"),
                new Uri(redirectUri));

            string code = authResult?.Properties["code"];
            if (string.IsNullOrEmpty(code))
            {
                await DisplayAlert("Помилка Google", "Не вдалося отримати код авторизації.", "OK");
                return;
            }

            var tokenRequestPayload = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" },
                { "code_verifier", codeVerifier }
            };

            using var httpTokenClient = new HttpClient();
            var tokenResponse = await httpTokenClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequestPayload));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                string errorContent = await tokenResponse.Content.ReadAsStringAsync();
                await DisplayAlert("Помилка Google Token", $"Не вдалося обміняти код на токен: {errorContent}", "OK");
                return;
            }

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
            using var tokenDoc = JsonDocument.Parse(tokenJson);
            var googleIdToken = tokenDoc.RootElement.GetProperty("id_token").GetString();

            // Прибираємо DisplayAlert, бо він не потрібен для робочого процесу
            // await DisplayAlert("ID Token", googleIdToken, "OK"); 

            if (string.IsNullOrEmpty(googleIdToken))
            {
                await DisplayAlert("Помилка Google Token", "Не вдалося отримати id_token з відповіді.", "OK");
                return;
            }

            var (backendLoginResult, backendError) = await _apiService.LoginWithGoogleAsync(googleIdToken);

            if (backendError != null)
            {
                await DisplayAlert("Помилка входу", backendError, "OK");
            }
            else if (backendLoginResult != null && !string.IsNullOrEmpty(backendLoginResult.accessToken))
            {
                await SecureStorage.SetAsync("auth_token", backendLoginResult.accessToken);
                await SecureStorage.SetAsync("user_id", backendLoginResult.userId);

                // ЦЕЙ РЯДОК ВЖЕ ПРАВИЛЬНИЙ.
                // Він замінює всю сторінку (NavigationPage(Login)) на ваш AppShell.
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await DisplayAlert("Помилка входу", "Не вдалося увійти через Google (невідома відповідь сервера).", "OK");
            }
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Вхід через Google скасовано.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Критична помилка", $"Сталася помилка: {ex.Message}", "ОК");
        }
    }

    private async void OnEmailLoginTapped(object sender, TappedEventArgs e)
    {
        // ❌ БУЛО (падало, бо Shell не існує):
        // await Shell.Current.GoToAsync(nameof(MailLogin));

        // ✅ СТАЛО (працює, бо ми в NavigationPage):
        await Navigation.PushAsync(new MailLogin());
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        // ❌ БУЛО (падало, бо Shell не існує):
        // await Shell.Current.GoToAsync(nameof(SignUp));

        // ✅ СТАЛО (працює, бо ми в NavigationPage):
        await Navigation.PushAsync(new SignUp());
    }
}