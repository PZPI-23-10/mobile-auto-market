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
    // Допоміжна функція для парсингу email з JWT (id_token)
    /*private string ParseEmailFromIdToken(string idToken)
    {
        try
        {
            // JWT складається з header.payload.signature
            // Нам потрібна середня частина (payload)
            string payloadJson = idToken.Split('.')[1];

            // Декодуємо з Base64Url
            payloadJson = payloadJson.Replace('-', '+').Replace('_', '/');
            payloadJson = payloadJson.PadRight(payloadJson.Length + (4 - payloadJson.Length % 4) % 4, '=');
            var payloadBytes = Convert.FromBase64String(payloadJson);
            string decodedJson = Encoding.UTF8.GetString(payloadBytes);

            // Дістаємо поле 'email'
            using var jsonDoc = JsonDocument.Parse(decodedJson);
            return jsonDoc.RootElement.GetProperty("email").GetString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ParseEmailFromIdToken] Помилка декодування: {ex.Message}");
            return null; // Повертаємо null, якщо сталася помилка
        }
    }*/

    private async void OnGoogleLoginTapped(object sender, TappedEventArgs e)
    {
        try
        {
            // --- КРОК 1-3: (Твій код отримання токена від Google) ---
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

            if (string.IsNullOrEmpty(googleIdToken))
            {
                await DisplayAlert("Помилка Google Token", "Не вдалося отримати id_token.", "OK");
                return;
            }

            // --- КРОК 4: (Твоя ідея) Дістаємо email і перевіряємо, чи юзер новий ---
            string googleEmail = ParseEmailFromIdToken(googleIdToken);
            if (string.IsNullOrEmpty(googleEmail))
            {
                await DisplayAlert("Помилка", "Не вдалося отримати email з Google токена.", "OK");
                return;
            }

            bool? emailExists = await _apiService.CheckEmailExistsAsync(googleEmail);
            if (emailExists == null)
            {
                await DisplayAlert("Помилка", "Не вдалося зв'язатися з сервером. Спробуйте пізніше.", "OK");
                return;
            }
            bool isExistingUser = emailExists.Value; // true = старий, false = новий

            // --- КРОК 5: Логінимося на НАШОМУ БЕКЕНДІ ---
            var (backendLoginResult, backendError) = await _apiService.LoginWithGoogleAsync(googleIdToken);

            // --- КРОК 6: Обробляємо відповідь від НАШОГО БЕКЕНДА ---
            if (backendError != null)
            {
                await DisplayAlert("Помилка входу", backendError, "OK");
            }
            else if (backendLoginResult != null && !string.IsNullOrEmpty(backendLoginResult.accessToken))
            {
                // --- КРОК 7: Зберігаємо токени ---
                await SecureStorage.SetAsync("auth_token", backendLoginResult.accessToken);
                await SecureStorage.SetAsync("user_id", backendLoginResult.userId);
                await SecureStorage.SetAsync("user_email", googleEmail);

                // --- ✅ КРОК 8 (ВИПРАВЛЕНО): Вирішуємо, куди йти ---
                if (isExistingUser)
                {
                    // СТАРИЙ КОРИСТУВАЧ: на головну
                    // (Припускаю, що твій головний екран - це AppShell)
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    // НОВИЙ КОРИСТУВАЧ: на редагування профілю
                    await DisplayAlert("Вітаємо!", "Схоже, ви тут вперше. Будь ласка, заповніть деталі профілю.", "OK");

                    // Використовуємо твій старий, правильний код для навігації
                    Application.Current.MainPage = new NavigationPage(new ProfileEdit());
                }
            }
            else
            {
                await DisplayAlert("Помилка входу", "Не вдалося увійти (невідома відповідь сервера).", "OK");
            }
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Вхід через Google скасовано.");
        }
        catch (Exception ex)
        {
            // Тут ми і ловимо NullReferenceException
            await DisplayAlert("Критична помилка", $"Сталася помилка: {ex.Message}", "ОК");
        }
    }

    // Не забудь також додати цю допоміжну функцію в твій клас Login.xaml.cs
    private string ParseEmailFromIdToken(string idToken)
    {
        try
        {
            string payloadJson = idToken.Split('.')[1];
            payloadJson = payloadJson.Replace('-', '+').Replace('_', '/');
            payloadJson = payloadJson.PadRight(payloadJson.Length + (4 - payloadJson.Length % 4) % 4, '=');
            var payloadBytes = Convert.FromBase64String(payloadJson);
            string decodedJson = Encoding.UTF8.GetString(payloadBytes);
            using var jsonDoc = JsonDocument.Parse(decodedJson);
            return jsonDoc.RootElement.GetProperty("email").GetString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ParseEmailFromIdToken] Помилка декодування: {ex.Message}");
            return null;
        }
    }
    /*private async void OnGoogleLoginTapped(object sender, TappedEventArgs e)
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

            // 1. Відкриваємо вікно Google Sign-In
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri("https://accounts.google.com/o/oauth2/v2/auth" +
                        $"?client_id={clientId}" +
                        $"&redirect_uri={redirectUri}" +
                        "&response_type=code" +
                        "&scope=openid%20email%20profile" + // Стандартні скоупи
                        $"&code_challenge={codeChallenge}" + // Додаємо PKCE
                        "&code_challenge_method=S256"),      // Метод для PKCE
                new Uri(redirectUri)); // Сюди Google поверне нас

            // 2. Отримуємо Authorization Code від Google
            string code = authResult?.Properties["code"];
            if (string.IsNullOrEmpty(code))
            {
                await DisplayAlert("Помилка Google", "Не вдалося отримати код авторизації.", "OK");
                return;
            }

            // 3. Обмінюємо Code на ID Token у Google
            var tokenRequestPayload = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" },
            { "code_verifier", codeVerifier } // Додаємо verifier для PKCE
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
            // Нас цікавить саме id_token, бо його чекає твій бекенд
            var googleIdToken = tokenDoc.RootElement.GetProperty("id_token").GetString();
            await Clipboard.SetTextAsync(googleIdToken);
            await DisplayAlert("ID Token", googleIdToken, "OK");
            Debug.WriteLine(googleIdToken);
            if (string.IsNullOrEmpty(googleIdToken))
            {
                await DisplayAlert("Помилка Google Token", "Не вдалося отримати id_token з відповіді.", "OK");
                return;
            }

            // 4. Відправляємо id_token на НАШ БЕКЕНД
            // ✅ ВАЖЛИВО: Переконайся, що в тебе створено _apiService, як ми робили для логіна/реєстрації
            var (backendLoginResult, backendError) = await _apiService.LoginWithGoogleAsync(googleIdToken);

            // 5. Обробляємо відповідь від НАШОГО БЕКЕНДА
            if (backendError != null)
            {
                // ЯКЩО БЕКЕНД ПОВЕРНУВ ПОМИЛКУ - ПОКАЗУЄМО ЇЇ
                await DisplayAlert("Помилка входу", backendError, "OK");
            }
            else if (backendLoginResult != null && !string.IsNullOrEmpty(backendLoginResult.accessToken)) // Перевір назву поля токена
            {
                // ЯКЩО УСПІХ
                await SecureStorage.SetAsync("auth_token", backendLoginResult.accessToken);
                await SecureStorage.SetAsync("user_id", backendLoginResult.userId);

                await DisplayAlert("Успішно", "Вхід через Google виконано!", "OK");
                Application.Current.MainPage = new NavigationPage(new ProfileEdit());
            }
            else
            {
                // Рідкісний випадок: помилки нема, але й даних нема
                await DisplayAlert("Помилка входу", "Не вдалося увійти через Google (невідома відповідь сервера).", "OK");
            }
        }
        catch (TaskCanceledException)
        {
            // Користувач сам закрив вікно входу
            Debug.WriteLine("Вхід через Google скасовано.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Критична помилка", $"Сталася помилка: {ex.Message}", "ОК");
        }
    }*/
    private async void OnEmailLoginTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new MailLogin());
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {

        await Navigation.PushAsync(new SignUp());
    }
}