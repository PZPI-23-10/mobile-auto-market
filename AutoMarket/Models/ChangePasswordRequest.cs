namespace AutoMarket.Models
{
    public class ChangePasswordRequest
    {
        // Назви з великої літери, бо так у твоєму API
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string PasswordConfirmation { get; set; }
    }
}