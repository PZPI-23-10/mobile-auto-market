using System.Text.Json.Serialization;

namespace AutoMarket.Models
{
    public class ColorDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        // Сподіваюсь, сервер повертає HEX код (наприклад "#FFFFFF")
        // Якщо ні - доведеться хардкодити кольори на клієнті
        [JsonPropertyName("hex")]
        public string HexCode { get; set; }

        // Властивість для XAML, щоб якщо HexCode нема, то був сірий
        [JsonIgnore]
        public Color ColorValue
        {
            get
            {
                if (string.IsNullOrEmpty(HexCode)) return Colors.Gray;
                try { return Color.FromArgb(HexCode); }
                catch { return Colors.Gray; }
            }
        }
    }
}