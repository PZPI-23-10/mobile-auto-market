using System.Text.Json.Serialization;

namespace AutoMarket.Models
{
    // Відповідь від самого Cloudinary
    public class CloudinaryResponse
    {
        public string secure_Url { get; set; }
    }
}