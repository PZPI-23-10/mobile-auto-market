
namespace AutoMarket.Models
{
    public class GoogleLoginRequest
    {
        public string googleToken { get; set; }
        public bool rememberMe { get; set; } = true;
    }
}