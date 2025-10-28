using System.Text.Json.Serialization;

namespace AutoMarket.Models
{
 
    public class LoginResponse
    {
        
        public string accessToken { get; set; }

        
        public string userId { get; set; }

        public string email { get; set; }

    }
}