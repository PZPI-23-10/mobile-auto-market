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
       
        private readonly string _baseUrl = "https://backend-auto-market.onrender.com";


        public ApiService()
        {
            _httpClient = new HttpClient();
           
        
        
            
        }


       
        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            string url = $"{_baseUrl}/api/Auth/register";
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

            string url = $"{_baseUrl}/api/Auth/login";

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
            string url = $"{_baseUrl}/api/Auth?userId={userId}";
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

        // ==========================================================
        // == КОД, ЯКИЙ ПОТРІБНО ДОДАТИ (ФІЛЬТРИ ТА ОГОЛОШЕННЯ) ==
        // ==========================================================

        // Це наш "майстер-список" (заглушка замість БД)
        // Коли API буде готовий, ми його видалимо.
        private List<CarListing> _allCars = new List<CarListing>
        {
        new CarListing
        {
            Title = "Lexus GX 470 2007", ImageUrl = "https://hdpic.club/uploads/posts/2021-12/thumbs/1640655886_2-hdpic-club-p-leksus-470-gx-2.jpg",
            PriceUSD = 13499, PriceUAH = 568443, Mileage = 191000, FuelType = "Газ/Бензин",
            Location = "Харків", Transmission = "Автомат", PostedDate = "3 дні тому"
        },
        new CarListing
        {
            Title = "Skoda Octavia A8 2021", ImageUrl = "https://i.infocar.ua/i/2/6018/119011/1920x.jpg",
            PriceUSD = 21500, PriceUAH = 905150, Mileage = 42000, FuelType = "Дизель",
            Location = "Київ", Transmission = "Автомат", PostedDate = "1 день тому"
        },
        new CarListing
        {
            Title = "Volkswagen ID.4 2022", ImageUrl = "https://www.edmunds.com/assets/m/volkswagen/id4/2021/oem/2021_volkswagen_id4_4dr-suv_awd-pro-s-statement_fq_oem_1_600.jpg",
            PriceUSD = 26900, PriceUAH = 1132190, Mileage = 0, FuelType = "Електро",
            Location = "Львів", Transmission = "Автомат", PostedDate = "Сьогодні"
        },
        new CarListing
        {
            Title = "BMW X5 2012", ImageUrl = "https://cars.ua/thumb/car/20210504/w933/h622/q80/kupit-bmw-x5-kiev-2723523.jpeg",
            PriceUSD = 18000, PriceUAH = 757800, Mileage = 240000, FuelType = "Дизель",
            Location = "Одеса", Transmission = "Автомат", PostedDate = "5 днів тому"
        }
        };



        // --- 2. ЛОГІКА ДЛЯ СПИСКУ АВТО (ЗАГЛУШКА) ---
        public async Task<List<CarListing>> GetListingsAsync(string fuelType, int? vehicleTypeId, int? makeId, int? modelId, string condition)
        {
            // 1. ЗАВАНТАЖУЄМО ВСІ ОГОЛОШЕННЯ З СЕРВЕРА
            List<CarListing> allCars;
            try
            {
                // !! РЕАЛЬНИЙ ЗАПИТ !!
                // TODO: Переконайтеся, що бекенд повертає CarListing у правильному форматі
                string url = $"{_baseUrl}/api/VehicleListing";
                allCars = await _httpClient.GetFromJsonAsync<List<CarListing>>(url);
            }
            catch (Exception ex)
            {
                // Якщо помилка (немає інтернету або API впав), повертаємо порожній список
                Debug.WriteLine($"ПОМИЛКА завантаження /api/VehicleListing: {ex.Message}");
                allCars = new List<CarListing>();
            }

            // 2. ФІЛЬТРУЄМО СПИСОК НА ТЕЛЕФОНІ (КЛІЄНТСЬКА ФІЛЬТРАЦІЯ)
            IEnumerable<CarListing> filtered = allCars;

            // Фільтр по пальному (поки що з заглушки)
            if (!string.IsNullOrEmpty(fuelType))
            {
                filtered = filtered.Where(c => c.FuelType == fuelType);
            }

            // Фільтр по стану (Нові/Вживані)
            if (condition == "Нові")
            {
                filtered = filtered.Where(c => c.Mileage == 0);
            }
            else if (condition == "Вживані")
            {
                filtered = filtered.Where(c => c.Mileage > 0);
            }
            if (vehicleTypeId.HasValue)
            {
                filtered = filtered.Where(c => c.VehicleTypeId == vehicleTypeId.Value);
            }

            // TODO: Додайте сюди решту логіки фільтрації (MakeId, ModelId...),
            // коли ви додасте ці властивості до вашої моделі CarListing.

            return filtered.ToList();
        }

        // Тип транспорту
        // --- 2. ЛОГІКА ДЛЯ ФІЛЬТРІВ (РЕАЛЬНИЙ API) ---

        // Тип транспорту
        public async Task<List<VehicleType>> GetVehicleTypesAsync() // TODO: Створіть клас VehicleType у Models
        {
            string url = $"{_baseUrl}/api/VehicleType";
            return await _httpClient.GetFromJsonAsync<List<VehicleType>>(url);
        }

        // Марка
        public async Task<List<Make>> GetMakesAsync()
        {
            string url = $"{_baseUrl}/api/VehicleBrand";
            return await _httpClient.GetFromJsonAsync<List<Make>>(url);
        }

        // Модель
        public async Task<List<Model>> GetModelsAsync(int makeId)
        {
            // У вас немає /api/VehicleModel?makeId=...
            // Тому ми завантажуємо ВСІ моделі і фільтруємо їх на телефоні
            string url = $"{_baseUrl}/api/VehicleModel";
            var allModels = await _httpClient.GetFromJsonAsync<List<Model>>(url);

            // Фільтруємо по MakeId
            return allModels.Where(m => m.MakeId == makeId).ToList();
        }

        // Стан
        public async Task<List<VehicleCondition>> GetConditionsAsync() // TODO: Створіть клас VehicleCondition у Models
        {
            string url = $"{_baseUrl}/api/VehicleCondition";
            return await _httpClient.GetFromJsonAsync<List<VehicleCondition>>(url);
        }

        // Регіон
        public async Task<List<AutoMarket.Models.Region>> GetRegionsAsync() // TODO: Створіть клас Region у Models
        {
            string url = $"{_baseUrl}/api/Region";
            return await _httpClient.GetFromJsonAsync<List<AutoMarket.Models.Region>>(url);
        }

        // --- ЗАГЛУШКИ, ЯКИХ НЕ ВИСТАЧАЄ В API ---

        public async Task<List<string>> GetFuelTypesAsync()
        {
            await Task.Delay(100); // Залишаємо заглушку
            return new List<string> { "Бензин", "Дизель", "Газ/Бензин", "Електро" };
        }

        public async Task<List<string>> GetTransmissionTypesAsync()
        {
            await Task.Delay(100); // Залишаємо заглушку
            return new List<string> { "Автомат", "Механіка", "Варіатор" };
        }

    }





    
}