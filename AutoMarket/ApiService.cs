using AutoMarket.Models;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AutoMarket
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
       
        private readonly string _baseUrl = "https://backend-auto-market.onrender.com/api/Account";

        public ApiService()
        {
            _httpClient = new HttpClient();
           
        
        
            
        }


       
        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            string url = $"{_baseUrl}/register";
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, request);

                if (response.IsSuccessStatusCode)
                {
                    return null; // ✅ Успіх! Повертаємо null
                }
                else
                {
                    // ❗ Помилка! Повертаємо текст помилки з сервера
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка реєстрації: {error}");
                    return error; // ✅ Повертаємо саму помилку
                }
            }
            catch (Exception ex)
            {
                // Критична помилка (напр., немає інтернету)
                Debug.WriteLine($"Критична помилка: {ex.Message}");
                return $"Помилка підключення: {ex.Message}"; // ✅ Повертаємо помилку підключення
            }
        }



       
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var loginRequest = new LoginRequest
            {
                email = email,
                password = password,
                rememberMe = true
            };

            string url = $"{_baseUrl}/login";

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    // Успіх! Читаємо відповідь у наш клас LoginResponse
                    LoginResponse loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return loginResponse;
                }
                else
                {
                    // Помилка (невірний пароль)
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка логіна: {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка логіна: {ex.Message}");
                return null;
            }
        }





        public async Task<(UserProfile Profile, string Error)> GetUserProfileAsync(string userId, string token)
        {
            string url = $"{_baseUrl}?userId={userId}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Успіх! Повертаємо профіль і null для помилки
                    UserProfile profile = await response.Content.ReadFromJsonAsync<UserProfile>();
                    return (profile, null);
                }
                else
                {
                    // Помилка! Повертаємо null для профілю і текст помилки
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка завантаження профілю: {error}");
                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                // Критична помилка! Повертаємо null для профілю і текст помилки
                Debug.WriteLine($"Критична помилка профілю: {ex.Message}");
                return (null, $"Помилка підключення: {ex.Message}");
            }
        }



        public async Task<string> UpdateUserProfileAsync(EditProfileRequest profileData, string token)
        {
            string userId = await SecureStorage.GetAsync("user_id");
            string url = $"{_baseUrl}/edit?userId={userId}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(profileData);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                // Логування статусу та тіла завжди
                Debug.WriteLine($"[UpdateUserProfileAsync] HTTP Status: {(int)response.StatusCode}");
                Debug.WriteLine($"[UpdateUserProfileAsync] Response Body: '{responseContent}'");

                if (response.IsSuccessStatusCode)
                {
                    return responseContent; // навіть якщо порожнє
                }
                else
                {
                    string errorMessage = $"Статус: {(int)response.StatusCode}. Повідомлення: {responseContent}";
                    Debug.WriteLine($"Помилка оновлення профілю: {errorMessage}");
                    return string.IsNullOrWhiteSpace(responseContent)
                        ? $"Помилка сервера (Статус: {(int)response.StatusCode})"
                        : errorMessage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка оновлення профілю: {ex.Message}");
                return $"Помилка підключення: {ex.Message}";
            }
        }



        public async Task<bool> SendVerificationEmailAsync(string email)
        {
            string url = $"{_baseUrl}/send-verification-email?email={Uri.EscapeDataString(email)}";

            try
            {
                Debug.WriteLine("=== Відправка коду підтвердження ===");
                Debug.WriteLine($"URL: {url}");
                Debug.WriteLine($"Метод: POST");
                string token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response = await _httpClient.PostAsync(url, null);

                // Додаємо логування
                Debug.WriteLine($"Status code: {(int)response.StatusCode} {response.StatusCode}");
                string rawResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response body: {rawResponse}");

                // Тепер враховуємо можливі успішні коди (200 або 204)
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка відправки коду: {ex.Message}");
                return false;
            }
        }


        /* public async Task<string> UpdateUserProfileAsync(EditProfileRequest profileData, string token)
         {
             string url = $"{_baseUrl}/edit";
             try
             {
                 var request = new HttpRequestMessage(HttpMethod.Post, url);
                 request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                 request.Content = JsonContent.Create(profileData);

                 HttpResponseMessage response = await _httpClient.SendAsync(request);

                 if (response.IsSuccessStatusCode)
                 {
                     return null; 
                 }
                 else
                 {

                     string error = await response.Content.ReadAsStringAsync();
                     Debug.WriteLine($"Помилка оновлення профілю: {error}");
                     return error;
                 }
             }
             catch (Exception ex)
             {
                 Debug.WriteLine($"Критична помилка оновлення профілю: {ex.Message}");
                 return $"Помилка підключення: {ex.Message}";
             }
         }*/


        public async Task<(string Url, string Error)> UploadToCloudinaryAsync(string filePath)
        {
            string cloudName = "dbazwsili";
            string uploadPreset = "automarket_app";
            string url = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload?upload_preset={uploadPreset}";

            try
            {
                
                using var fileStream = File.OpenRead(filePath);
                using var streamContent = new StreamContent(fileStream);

               
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                using var multipartFormContent = new MultipartFormDataContent();

                // Додаємо сам файл. Третій параметр - це ім'я файлу, яке побачить сервер.
                multipartFormContent.Add(streamContent, "file", Path.GetFileName(filePath));

                // Додаємо назву пресету
                /*multipartFormContent.Add(new StringContent(uploadPreset), "upload_preset");*/

                // Відправляємо запит
                var response = await _httpClient.PostAsync(url, multipartFormContent);

                if (response.IsSuccessStatusCode)
                {
                    var cloudinaryResponse = await response.Content.ReadFromJsonAsync<CloudinaryResponse>();
                    // Перевір назву властивості у Models/CloudinaryResponse.cs
                    return (cloudinaryResponse.secure_Url, null);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine($"Помилка Cloudinary (File Upload): {error}");
                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка Cloudinary (File Upload): {ex.Message}");
                return (null, $"Помилка підключення: {ex.Message}");
            }
        }
        /*public async Task<(string Url, string Error)> UploadToCloudinaryAsync(string filePath)
        {
            string cloudName = "dbazwsili";
            string uploadPreset = "automarket_app";
            string url = $"https://api.cloudinary.com/v1_1/{cloudName}/image/upload";
            try
            {
                // 1. Читаємо файл і конвертуємо в Base64 рядок
                byte[] imageBytes = File.ReadAllBytes(filePath);
                string base64Image = Convert.ToBase64String(imageBytes);

                // Додаємо префікс, який потрібен Cloudinary для Base64
                string dataUri = $"data:image/jpeg;base64,{base64Image}";

                // 2. Створюємо простий JSON-об'єкт для відправки
                var payload = new
                {
                    file = dataUri, // Відправляємо Base64 рядок як "file"
                    upload_preset = uploadPreset,
                   
                };

                // 3. Відправляємо як звичайний JSON POST-запит
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, payload);

                if (response.IsSuccessStatusCode)
                {
                    var cloudinaryResponse = await response.Content.ReadFromJsonAsync<CloudinaryResponse>();
                    return (cloudinaryResponse.secure_Url, null); // Або SecureUrl, перевір свій CloudinaryResponse.cs
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка Cloudinary (Base64): {error}");
                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка Cloudinary (Base64): {ex.Message}");
                return (null, $"Помилка підключення: {ex.Message}");
            }*/


        /* try
         {
             using var fileStream = File.OpenRead(filePath);
             using var streamContent = new StreamContent(fileStream);
             streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
             using var multipartFormContent = new MultipartFormDataContent();
             multipartFormContent.Add(streamContent, "file", Path.GetFileName(filePath));
             multipartFormContent.Add(new StringContent(uploadPreset), "upload_preset");

             var response = await _httpClient.PostAsync(url, multipartFormContent);

             if (response.IsSuccessStatusCode)
             {
                 var cloudinaryResponse = await response.Content.ReadFromJsonAsync<CloudinaryResponse>();
                 return (cloudinaryResponse.secure_Url, null);
             }
             else
             {
                 string error = await response.Content.ReadAsStringAsync();
                 Debug.WriteLine($"Помилка Cloudinary: {error}");
                 return (null, error);
             }
         }
         catch (Exception ex)
         {
             Debug.WriteLine($"Критична помилка Cloudinary: {ex.Message}");
             return (null, $"Помилка підключення: {ex.Message}");
         }*/


        public async Task<bool> VerifyEmailCodeAsync(string email, string code, string token = null)
        {
            string url = $"{_baseUrl}/verify-email";
            // Якщо потрібен токен – додаємо:
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // Підготовка тіла запиту (JSON):
            var body = new
            {
                email = email,
                code = code
            };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(url, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"VerifyEmailCodeAsync — Status: {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine($"VerifyEmailCodeAsync — Body: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }







        // Це ДОДАТИ в ApiService.cs
        /*public async Task<LoginResponse> LoginWithGoogleAsync(string googleToken)
        {
            var requestData = new GoogleLoginRequest
            {
                googleToken = googleToken,
                rememberMe = true
            };

            // ❗ Переконайся, що цей URL правильний для твого бекенда
            string url = $"{_baseUrl}/web/google";

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestData);

                if (response.IsSuccessStatusCode)
                {
                    // Бекенд має повернути той самий LoginResponse (з токеном/ID)
                    LoginResponse loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return loginResponse;
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка Google логіна на бекенді: {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка Google логіна: {ex.Message}");
                return null;
            }
        }*/
        // --- МЕТОД GOOGLE ЛОГІНА (ОНОВЛЕНИЙ) ---
        // Повертає кортеж: (Відповідь, Помилка)
        public async Task<(LoginResponse Response, string Error)> LoginWithGoogleAsync(string googleToken)
        {
            var requestData = new GoogleLoginRequest
            {
                googleToken = googleToken,
                rememberMe = true
            };

            string url = $"{_baseUrl}/web/google";

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestData);

                if (response.IsSuccessStatusCode)
                {
                    // УСПІХ: Повертаємо відповідь і null для помилки
                    LoginResponse loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return (loginResponse, null);
                }
                else
                {
                    // ПОМИЛКА: Повертаємо null для відповіді і текст помилки
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка Google логіна на бекенді: {error}");
                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                // КРИТИЧНА ПОМИЛКА: Повертаємо null і помилку підключення
                Debug.WriteLine($"Критична помилка Google логіна: {ex.Message}");
                return (null, $"Помилка підключення: {ex.Message}");
            }
        }


        

        public async Task<string> ChangePasswordAsync(ChangePasswordRequest requestData, string token)
        {
           
            string url = $"{_baseUrl}/ChangePassword";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(requestData);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return null; // Успіх
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка зміни пароля: {error}");
                    // Спробуємо витягти 'message', якщо є
                    if (!string.IsNullOrEmpty(error)) { try { var jsonDoc = JsonDocument.Parse(error); if (jsonDoc.RootElement.TryGetProperty("message", out var msg)) { error = msg.GetString(); } } catch { } }
                    return string.IsNullOrEmpty(error) ? $"Помилка сервера (Статус: {(int)response.StatusCode})" : error;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка зміни пароля: {ex.Message}");
                return $"Помилка підключення: {ex.Message}";
            }
        }

    }





    
}