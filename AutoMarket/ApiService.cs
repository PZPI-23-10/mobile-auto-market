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
       
        private readonly string _baseUrl = "https://backend-auto-market-wih5h.ondigitalocean.app/api";

        private readonly System.Text.Json.JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public ApiService()
        {
            _httpClient = new HttpClient();
            
        }


       
        public async Task<string> RegisterAsync(RegisterRequest request)
        {
            string url = $"{_baseUrl}/Auth/register";
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

            string url = $"{_baseUrl}/Auth/android/google";

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

        // --------------------------------------------------------------------------------------- ниже для вивода авто api 

        public async Task<CarCheckInfo> CheckCarByNumberAsync(string licensePlate)
        {
            // 1. Чистимо номер від пробілів та кирилиці/латиниці (API хоче чистий рядок)
            // Але для простоти передамо як є, зазвичай API розумне.
            // Краще прибрати пробіли.
            string cleanNumber = licensePlate.Replace(" ", "").ToUpper();

            // 2. URL сервісу
            string apiKey = "27c52a7ec72f857964951cbd9f06cf0e"; // Твій ключ
            string url = $"https://baza-gai.com.ua/nomer/{cleanNumber}";

            try
            {
                // 3. Створюємо запит з заголовком
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("X-Api-Key", apiKey);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CarCheckInfo>();
                    return result;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"GAI API Error: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GAI API Exception: {ex.Message}");
                return null;
            }
        }

        // 1. Отримати Типи транспорту
        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            try
            {
                // Формуємо повне посилання: .../api/VehicleType
                string url = $"{_baseUrl}/VehicleType";
                return await _httpClient.GetFromJsonAsync<List<VehicleTypeDto>>(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка отримання типів: {ex.Message}");
                return new List<VehicleTypeDto>(); // Повертаємо пустий список, щоб програма не впала
            }
        }

        // 2. Отримати Марки (залежить від Типу транспорту)
        public async Task<List<VehicleBrandDto>> GetBrandsByTypeAsync(int typeId)
        {
            try
            {
                string url = $"{_baseUrl}/VehicleBrand/for-type/{typeId}";
                return await _httpClient.GetFromJsonAsync<List<VehicleBrandDto>>(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка отримання марок: {ex.Message}");
                return new List<VehicleBrandDto>();
            }
        }

        // Оновлений метод: фільтрує і по Бренду, і по Типу
        public async Task<List<VehicleModelDto>> GetModelsByBrandAsync(int brandId, int typeId)
        {
            // Додаємо vehicleTypeId у запит
            string url = $"{_baseUrl}/VehicleModel?brandId={brandId}&vehicleTypeId={typeId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var result = System.Text.Json.JsonSerializer.Deserialize<List<VehicleModelDto>>(jsonResponse, options);
                    return result ?? new List<VehicleModelDto>();
                }
                else
                {
                    return new List<VehicleModelDto>();
                }
            }
            catch
            {
                return new List<VehicleModelDto>();
            }
        }

        // 4. Отримати Регіони
        public async Task<List<RegionDto>> GetRegionsAsync()
        {
            try
            {
                string url = $"{_baseUrl}/Region";
                return await _httpClient.GetFromJsonAsync<List<RegionDto>>(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка отримання регіонів: {ex.Message}");
                return new List<RegionDto>();
            }
        }

        // 5. Отримати Міста (залежить від Регіону)
        public async Task<List<CityDto>> GetCitiesByRegionAsync(int regionId)
        {
            try
            {
                string url = $"{_baseUrl}/City/for-region/{regionId}";
                return await _httpClient.GetFromJsonAsync<List<CityDto>>(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка отримання міст: {ex.Message}");
                return new List<CityDto>();
            }
        }

        // 6. Отримати Пальне
        public async Task<List<FuelTypeDto>> GetFuelTypesAsync()
        {
            try
            {
                string url = $"{_baseUrl}/FuelType";
                return await _httpClient.GetFromJsonAsync<List<FuelTypeDto>>(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка отримання пального: {ex.Message}");
                return new List<FuelTypeDto>();
            }
        }

        // 7. Отримати КПП
        public async Task<List<GearTypeDto>> GetGearTypesAsync()
        {
            try
            {
                string url = $"{_baseUrl}/GearType";
                return await _httpClient.GetFromJsonAsync<List<GearTypeDto>>(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Помилка отримання КПП: {ex.Message}");
                return new List<GearTypeDto>();
            }
        }








        // --- ГАРАНТОВАНЕ ОТРИМАННЯ СПИСКУ (БЕЗ ФІЛЬТРІВ) ---
        public async Task<List<CarListing>> GetAllListingsAsync()
        {
            // Перевір, чи точно Listing з великої. Якщо сервер Linux - це важливо.
            string url = $"{_baseUrl}/Listing";

            try
            {
                // 1. Отримуємо "сиру" відповідь
                var response = await _httpClient.GetAsync(url);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // --- ДІАГНОСТИКА (ДИВИСЬ У ВІКНО OUTPUT) ---
                System.Diagnostics.Debug.WriteLine("=================================");
                System.Diagnostics.Debug.WriteLine($"[LISTING API] STATUS: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[LISTING API] JSON: {jsonResponse}");
                System.Diagnostics.Debug.WriteLine("=================================");

                if (response.IsSuccessStatusCode)
                {
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var result = System.Text.Json.JsonSerializer.Deserialize<List<CarListing>>(jsonResponse, options);

                    System.Diagnostics.Debug.WriteLine($"[LISTING API] Розпізнано авто: {result?.Count ?? 0}");
                    return result ?? new List<CarListing>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LISTING API] ERROR: {response.ReasonPhrase}");
                    return new List<CarListing>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LISTING API] CRITICAL ERROR: {ex.Message}");
                return new List<CarListing>();
            }
        }

        // ... тут залиш інші свої методи (Login, Register, GetMakes...), не чіпай їх
        // Додай сюди методи для фільтрів (GetMakesAsync і т.д.) з минулих разів, 
        // якщо вони видалилися, але головне зараз - Listings.

        // Отримати Типи Кузова
        // (Можна брати всі, або якщо є ендпоінт /for-type/{id} - краще його, але поки візьмемо загальний)
        

        public async Task<List<BaseDto>> GetConditionsAsync()
        {
            string url = $"{_baseUrl}/VehicleCondition";
            try
            {
                return await _httpClient.GetFromJsonAsync<List<BaseDto>>(url);
            }
            catch { return new List<BaseDto>(); }
        }

        public async Task<List<BaseDto>> GetBodyTypesAsync()
        {
            string url = $"{_baseUrl}/VehicleBodyType";
            try
            {
                return await _httpClient.GetFromJsonAsync<List<BaseDto>>(url);
            }
            catch { return new List<BaseDto>(); }
        }

        public async Task<List<ColorDto>> GetColorsAsync()
        {
            string url = $"{_baseUrl}/Color";
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ColorDto>>(url);
            }
            catch { return new List<ColorDto>(); }
        }

        // --- МЕТОД СТВОРЕННЯ ОГОЛОШЕННЯ ---
        public async Task<bool> CreateListingAsync(
            int modelId,
            int bodyTypeId,
            int gearTypeId,
            int fuelTypeId,
            int conditionId,
            int cityId,
            int year,
            int mileage,
            string number,
            string colorHex,
            double price,
            string description,
            bool hasAccident,
            List<FileResult> photos)
        {
            string url = $"{_baseUrl}/Listing";

            try
            {
                using var content = new MultipartFormDataContent();

                // 1. Додаємо текстові/числові поля
                content.Add(new StringContent(modelId.ToString()), "ModelId");
                content.Add(new StringContent(bodyTypeId.ToString()), "BodyTypeId");
                content.Add(new StringContent(gearTypeId.ToString()), "GearTypeId");
                content.Add(new StringContent(fuelTypeId.ToString()), "FuelTypeId");
                content.Add(new StringContent(conditionId.ToString()), "ConditionId");
                content.Add(new StringContent(cityId.ToString()), "CityId");
                content.Add(new StringContent(year.ToString()), "Year");
                content.Add(new StringContent(mileage.ToString()), "Mileage");
                content.Add(new StringContent(number ?? ""), "Number"); // Номер може бути пустим
                content.Add(new StringContent(colorHex ?? "#FFFFFF"), "ColorHex");
                content.Add(new StringContent(price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price"); // Щоб була крапка, а не кома
                content.Add(new StringContent(description ?? ""), "Description");
                content.Add(new StringContent(hasAccident.ToString().ToLower()), "HasAccident"); // true/false

                // 2. Додаємо фотографії (ВАРІАНТ ЯК НА WEB)
                if (photos != null)
                {
                    // Використовуємо for замість foreach, бо нам потрібен індекс i (0, 1, 2...)
                    for (int i = 0; i < photos.Count; i++)
                    {
                        var file = photos[i];

                        // А. Читаємо файл у пам'ять
                        using var stream = await file.OpenReadAsync();
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        var fileBytes = memoryStream.ToArray();

                        // Б. Створюємо контент файлу
                        var fileContent = new ByteArrayContent(fileBytes);
                        string mimeType = file.ContentType ?? "image/jpeg";
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);

                        string fileName = file.FileName;
                        // Страховка на розширення
                        if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                            !fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) &&
                            !fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            fileName += ".jpg";
                        }

                        // В. !!! ГОЛОВНА ЗМІНА: Формуємо ключі як на WEB !!!

                        // 1. Сам файл: NewPhotos[0].File
                        string fileKey = $"NewPhotos[{i}].File";
                        content.Add(fileContent, fileKey, fileName);

                        // 2. Порядок сортування: NewPhotos[0].SortOrder
                        string sortOrderKey = $"NewPhotos[{i}].SortOrder";
                        content.Add(new StringContent(i.ToString()), sortOrderKey);

                        System.Diagnostics.Debug.WriteLine($"[UPLOAD WEB-STYLE] {fileKey} -> {fileName}");
                    }
                }

                // 3. Додаємо Токен (Авторизація)
                var token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // 4. Відправляємо
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[CREATE LISTING ERROR] {response.StatusCode}: {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CREATE LISTING CRITICAL] {ex.Message}");
                return false;
            }
        }

        public async Task<List<Chat>> GetMyChatsAsync(string token)
        {
            string url = $"{_baseUrl}/Chat/my";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var chats = await response.Content.ReadFromJsonAsync<List<Chat>>();
                    return chats ?? new List<Chat>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting chats: {ex.Message}");
            }

            return new List<Chat>(); // Повертаємо пустий список у разі помилки
        }

        public async Task<Chat> GetOrCreateChatAsync(int otherUserId, string token)
        {
            string url = $"{_baseUrl}/Chat/with/{otherUserId}";

            if (string.IsNullOrEmpty(token)) return null;

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Робимо запит
                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    // 1. Читаємо відповідь як рядок (щоб бачити, що там прийшло)
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Якщо сервер повернув пустоту, але код 200/201 - це дивно, але буває.
                    if (string.IsNullOrWhiteSpace(jsonResponse))
                    {
                        // Можна спробувати почекати 200мс і спробувати ще раз, якщо бекенд тупить
                        // Але поки просто повернемо null і подивимося логи
                        System.Diagnostics.Debug.WriteLine("⚠️ Сервер створив чат, але не повернув об'єкт.");
                        return null;
                    }

                    // 2. Використовуємо _serializerOptions (ВАЖЛИВО!)
                    // Це виправить проблему, якщо сервер шле "id", а ми чекаємо "Id"
                    var chat = System.Text.Json.JsonSerializer.Deserialize<Chat>(jsonResponse, _serializerOptions);
                    return chat;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ API Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error GetOrCreateChat: {ex.Message}");
            }

            return null;
        }



        // Метод для завантаження історії повідомлень
        public async Task<List<ChatMessageDto>> GetChatHistoryAsync(int chatId, string token)
        {
            string url = $"{_baseUrl}/Chat/{chatId}/history";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var messages = await response.Content.ReadFromJsonAsync<List<ChatMessageDto>>();
                    return messages ?? new List<ChatMessageDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
            }

            return new List<ChatMessageDto>();
        }


        // Отримати кількість непрочитаних повідомлень для конкретного чату
        public async Task<int> GetUnreadCountAsync(int chatId, string token)
        {
            string url = $"{_baseUrl}/Chat/{chatId}/unreadCount";

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetStringAsync(url);

                if (int.TryParse(response, out int count))
                {
                    return count;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching unread count: {ex.Message}");
            }

            return 0; // Якщо помилка - показуємо 0
        }

    }





    
}