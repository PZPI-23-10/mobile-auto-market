using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace AutoMarket.Models
{
    // Додаємо INotifyPropertyChanged, щоб оновлювати картинку "на льоту"
    public class CarListing : INotifyPropertyChanged
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("mileage")]
        public int Mileage { get; set; }

        [JsonPropertyName("licensePlate")]
        public string LicensePlate { get; set; }

        [JsonPropertyName("vin")]
        public string Vin { get; set; }

        [JsonPropertyName("photoUrls")]
        public List<ListingPhotoDto> Photos { get; set; }

        [JsonPropertyName("brand")]
        public NestedDto BrandObj { get; set; }

        [JsonPropertyName("model")]
        public NestedDto ModelObj { get; set; }

        [JsonPropertyName("city")]
        public NestedDto CityObj { get; set; }

        [JsonPropertyName("fuelType")]
        public NestedDto FuelObj { get; set; }

        [JsonPropertyName("gearType")]
        public NestedDto GearObj { get; set; }

        // --- Властивості для відображення ---

        [JsonIgnore] public string Brand => BrandObj?.Name ?? "Невідомо";
        [JsonIgnore] public string Model => ModelObj?.Name ?? "";
        [JsonIgnore] public string City => CityObj?.Name ?? "";
        [JsonIgnore] public string FuelType => FuelObj?.Name ?? "";
        [JsonIgnore] public string GearType => GearObj?.Name ?? "";
        [JsonIgnore] public string FullTitle => $"{Brand} {Model} {Year}";
        [JsonIgnore] public string PriceUsdFormatted => $"{Price:N0} $".Replace(",", " ");
        [JsonIgnore] public string PriceUahFormatted => $"{Price * 41.5m:N0} грн".Replace(",", " ");

        // --- ЛОГІКА ГОРТАННЯ ФОТО ---

        private int _currentPhotoIndex = 0;

        [JsonIgnore]
        public string CurrentPhotoUrl
        {
            get
            {
                if (Photos == null || Photos.Count == 0)
                    return "https://placehold.co/600x400/333333/white?text=No+Photo";

                // Беремо фото по поточному індексу
                string url = Photos[_currentPhotoIndex].Url;

                if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return url;
                if (!url.StartsWith("/")) url = "/" + url;
                return $"https://backend-auto-market.onrender.com{url}";
            }
        }

        // Властивість, щоб показувати стрілочки ТІЛЬКИ якщо фото більше одного
        [JsonIgnore]
        public bool HasMultiplePhotos => Photos != null && Photos.Count > 1;

        // Команди для кнопок
        [JsonIgnore]
        public ICommand NextPhotoCommand => new Command(() =>
        {
            if (!HasMultiplePhotos) return;

            // Збільшуємо індекс. Якщо кінець списку - йдемо на початок (0)
            _currentPhotoIndex++;
            if (_currentPhotoIndex >= Photos.Count) _currentPhotoIndex = 0;

            OnPropertyChanged(nameof(CurrentPhotoUrl)); // Оновити картинку
        });

        [JsonIgnore]
        public ICommand PrevPhotoCommand => new Command(() =>
        {
            if (!HasMultiplePhotos) return;

            // Зменшуємо індекс. Якщо початок - йдемо в кінець
            _currentPhotoIndex--;
            if (_currentPhotoIndex < 0) _currentPhotoIndex = Photos.Count - 1;

            OnPropertyChanged(nameof(CurrentPhotoUrl)); // Оновити картинку
        });


        // --- Реалізація INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class NestedDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
    }

    public class ListingPhotoDto
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("url")] public string Url { get; set; }
    }
}