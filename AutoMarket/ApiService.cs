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
       
        private readonly string _baseUrl = "https://backend-auto-market.onrender.com/api";

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

            string url = $"{_baseUrl}/auth/login";

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
            string url = $"{_baseUrl}/Profile?userId={userId}";
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    UserProfile profile = await response.Content.ReadFromJsonAsync<UserProfile>();
                    return (profile, null);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Помилка завантаження профілю: {error}");
                    return (null, error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка профілю: {ex.Message}");
                return (null, $"Помилка підключення: {ex.Message}");
            }
        }




   
        public async Task<string> UpdateUserProfileAsync(EditProfileRequest profileData,
                                                Stream photoStream, // Новий параметр
                                                string photoFileName, // Новий параметр
                                                string token)
        {
            
            string url = $"{_baseUrl}/Profile/update";
           

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 2. Створюємо 'multipart' контейнер
                var multipartContent = new MultipartFormDataContent();

                // 3. Додаємо всі ТЕКСТОВІ поля з profileData
                // Важливо: 'Key' (перший параметр) має точно відповідати тому, 
                // що ти використовував у Postman (firstName, lastName і т.д.)
                multipartContent.Add(new StringContent(profileData.firstName ?? ""), "firstName");
                multipartContent.Add(new StringContent(profileData.lastName ?? ""), "lastName");
                multipartContent.Add(new StringContent(profileData.phoneNumber ?? ""), "phoneNumber");
                string dateString = profileData.dateOfBirth.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                multipartContent.Add(new StringContent(dateString), "dateOfBirth");
                multipartContent.Add(new StringContent(profileData.address ?? ""), "address");
                multipartContent.Add(new StringContent(profileData.country ?? ""), "country");
                multipartContent.Add(new StringContent(profileData.aboutYourself ?? ""), "aboutYourself");

                // Якщо у тебе є поле Password (як у Swagger), додай його теж
                // if (!string.IsNullOrEmpty(profileData.Password))
                // {
                //     multipartContent.Add(new StringContent(profileData.Password), "Password"); // Або "password"
                // }

                // 4. Додаємо ФОТО (якщо воно є)
                if (photoStream != null && photoStream.Length > 0)
                {
                    // Створюємо контент для файлу
                    var fileContent = new StreamContent(photoStream);

                    // Встановлюємо Content-Type для файлу
                    // (можна зробити розумнішу логіку на основі photoFileName, але поки так)
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg"); // або "image/png"

                    // Додаємо файл у 'multipart' контейнер
                    // "Photo" - це 'Key' з API.
                    // 'photoFileName' - це ім'я файлу, яке побачить сервер.
                    multipartContent.Add(fileContent, "Photo", photoFileName);
                }

                // 5. Призначаємо наш зібраний 'multipart' контент як тіло запиту
                request.Content = multipartContent;

                // Далі твій код для відправки та обробки відповіді залишається без змін
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                Debug.WriteLine($"[UpdateUserProfileAsync] HTTP Status: {(int)response.StatusCode}");
                Debug.WriteLine($"[UpdateUserProfileAsync] Response Body: '{responseContent}'");

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(responseContent))
                        return "OK";

                    return responseContent;
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
            // Твоя оригінальна, ПРАВИЛЬНА URL
            string url = $"{_baseUrl}/auth/send-verification-email?email={Uri.EscapeDataString(email)}";

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

                // ... (решта коду залишається)

                Debug.WriteLine($"Status code: {(int)response.StatusCode} {response.StatusCode}");
                string rawResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response body: {rawResponse}");

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
        public async Task<bool> SendPasswordResetCodeAsync(string email)
        {
            // 1. Використовуємо ТОЙ САМИЙ URL, що й для верифікації
            string url = $"{_baseUrl}/auth/send-verification-email?email={Uri.EscapeDataString(email)}";

            try
            {
                Debug.WriteLine("=== Відправка коду СКИДАННЯ ПАРОЛЯ ===");
                Debug.WriteLine($"URL: {url}");
                Debug.WriteLine($"Метод: POST");

                // 2. Створюємо новий запит
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                // 3. ❗️ВАЖЛИВО: Ми НЕ додаємо 'Authorization' header.
                // Ми НЕ хочемо надсилати токен, навіть якщо він
                // випадково зберігся у _httpClient з минулих запитів.
                // Тому ми створюємо 'request' вручну, а не
                // використовуємо 'PostAsync(url, null)' напряму.

                // 4. Надсилаємо запит
                // (null означає, що тіло запиту порожнє)
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                Debug.WriteLine($"Status code: {(int)response.StatusCode} {response.StatusCode}");
                string rawResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response body: {rawResponse}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критична помилка SendPasswordResetCodeAsync: {ex.Message}");
                return false;
            }
        }



        /* public async Task<(string Url, string Error)> UploadToCloudinaryAsync(string filePath)
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
                 *//*multipartFormContent.Add(new StringContent(uploadPreset), "upload_preset");*//*

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
         }*/



        // Прибираємо 'email' з параметрів, він більше не потрібен
        public async Task<bool> VerifyEmailCodeAsync(string code, string token = null)
        {
            string url = $"{_baseUrl}/Auth/verify-email";

            // Якщо потрібен токен – додаємо:
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            // --- ГОЛОВНА ЗМІНА ТУТ ---
            // Сервер очікує не JSON-об'єкт, а просто JSON-рядок
            // JsonSerializer.Serialize(code) перетворить "123456" на "\"123456\""
            // Це і є "application/json" версія простого рядка
            var content = new StringContent(JsonSerializer.Serialize(code), Encoding.UTF8, "application/json");

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






        public async Task<(LoginResponse Response, string Error)> LoginWithGoogleAsync(string googleToken)
        {
            var requestData = new GoogleLoginRequest
            {
                googleToken = googleToken,
                rememberMe = true
            };

            string url = $"{_baseUrl}/auth/android/google";

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
           
            string url = $"{_baseUrl}/auth/change-password";
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


        // Прибираємо приватний клас EmailExistsResponse, він не потрібен

        public async Task<bool?> CheckEmailExistsAsync(string email)
        {
            string url = $"{_baseUrl}/auth/email-exists?email={Uri.EscapeDataString(email)}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // --- ОСЬ ВИПРАВЛЕННЯ ---
                    // Читаємо відповідь як простий рядок (який буде "true" або "false")
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Конвертуємо рядок "true" у bool true
                    if (bool.TryParse(responseBody, out bool emailExists))
                    {
                        return emailExists; // Поверне true або false
                    }

                    // Якщо сервер повернув щось дивне (не "true" і не "false")
                    Debug.WriteLine($"[CheckEmailExistsAsync] Незрозуміла відповідь: {responseBody}");
                    return null;
                }
                else
                {
                    // API повернуло помилку (500, 404 тощо)
                    Debug.WriteLine($"[CheckEmailExistsAsync] Помилка API: {response.StatusCode}");
                    return null; // Повертаємо null, щоб позначити помилку
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckEmailExistsAsync] Критична помилка: {ex.Message}");
                return null; // Помилка (немає інтернету тощо)
            }
        }

        public async Task<bool> ConfirmPasswordResetCodeAsync(string email, string code)
        {
            // Припускаємо, що параметри передаються у посиланні (query string)
            string url = $"{_baseUrl}/Auth/confirm-password-change?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(code)}";

            // Цей запит НЕ надсилає Bearer Token, бо юзер не залогінений

            try
            {
                Debug.WriteLine($"[ConfirmPasswordResetCodeAsync] URL: {url}");
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                Debug.WriteLine($"[ConfirmPasswordResetCodeAsync] Status: {response.StatusCode}");

                // Припускаємо, що 200 OK означає, що код вірний
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfirmPasswordResetCodeAsync] Critical Error: {ex.Message}");
                return false;
            }
        }

        
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            string url = $"{_baseUrl}/Auth/reset-password";

            // Цей запит не надсилає токен
            try
            {
                Debug.WriteLine($"[ResetPasswordAsync] URL: {url}");

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, request);

                Debug.WriteLine($"[ResetPasswordAsync] Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ResetPasswordAsync] Error: {error}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ResetPasswordAsync] Critical Error: {ex.Message}");
                return false;
            }
        }





    }





    
}