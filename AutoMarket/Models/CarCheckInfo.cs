using System.Text.Json.Serialization;

namespace AutoMarket.Models
{
    public class CarCheckInfo
    {
        [JsonPropertyName("digits")] public string Digits { get; set; }
        [JsonPropertyName("vin")] public string Vin { get; set; }
        [JsonPropertyName("vendor")] public string Vendor { get; set; }
        [JsonPropertyName("model")] public string Model { get; set; }
        [JsonPropertyName("model_year")] public int Year { get; set; }
        [JsonPropertyName("photo_url")] public string PhotoUrl { get; set; }

        // --- ГЛОБАЛЬНИЙ КОЛІР (іноді тут пусто) ---
        [JsonPropertyName("color")]
        public ColorInfo Color { get; set; }

        public class ColorInfo
        {
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("slug")] public string Slug { get; set; }
            [JsonPropertyName("ua")] public string Ua { get; set; } // Іноді буває українська назва
        }

        // --- ОПЕРАЦІЇ ---
        [JsonPropertyName("operations")]
        public List<OperationInfo> Operations { get; set; }

        // Отримати останню операцію
        [JsonIgnore]
        public OperationInfo LastOperation => (Operations != null && Operations.Count > 0) ? Operations[0] : null;

        // ==========================================
        //           ЛОГІКА ВІДОБРАЖЕННЯ
        // ==========================================

        [JsonIgnore]
        public string CleanColorName
        {
            get
            {
                // 1. Спочатку шукаємо в ОСТАННІЙ ОПЕРАЦІЇ (найточніше)
                string c = LastOperation?.Color?.Name
                        ?? LastOperation?.Color?.Ua
                        ?? LastOperation?.Color?.Slug;

                // 2. Якщо там пусто, шукаємо в глобальному об'єкті
                if (string.IsNullOrEmpty(c))
                {
                    c = Color?.Name ?? Color?.Ua ?? Color?.Slug;
                }

                // 3. Якщо все ще пусто - ставимо прочерк
                if (string.IsNullOrEmpty(c)) return "-";

                // 4. ЧИСТКА СМІТТЯ
                // Прибираємо "options.color.", "old.color." і т.д.
                if (c.Contains(".")) c = c.Split('.').Last();

                // Робимо красивим (Перша буква велика, решта малі, або все великими)
                return c.ToUpper();
            }
        }

        [JsonIgnore]
        public string LastOperationName => LastOperation?.OperationDetails?.NameUa ?? LastOperation?.Kind?.NameUa ?? "-";

        // Дата реєстрації (перевіряємо різні поля)
        [JsonIgnore]
        public string LastRegistrationDate => LastOperation?.Date ?? LastOperation?.RegisteredAt ?? "-";

        // Об'єм двигуна
        [JsonIgnore]
        public string EngineCapacity => LastOperation?.Capacity > 0 ? $"{LastOperation.Capacity} см³" : "-";
    }

    public class OperationInfo
    {
        [JsonPropertyName("date")] public string Date { get; set; }
        [JsonPropertyName("registered_at")] public string RegisteredAt { get; set; }

        [JsonPropertyName("displacement")] public int Capacity { get; set; }

        // !!! КОЛІР ВСЕРЕДИНІ ОПЕРАЦІЇ !!!
        [JsonPropertyName("color")] public CarCheckInfo.ColorInfo Color { get; set; }

        [JsonPropertyName("kind")] public KindInfo Kind { get; set; }
        public class KindInfo { [JsonPropertyName("ua")] public string NameUa { get; set; } }

        [JsonPropertyName("operation")] public OperationDetailsInfo OperationDetails { get; set; }
        public class OperationDetailsInfo
        {
            [JsonPropertyName("ua")] public string NameUa { get; set; }
        }
    }
}