using System.Text.Json.Serialization;
using Microsoft.Maui.Controls.Internals;
namespace AutoMarket.Models
{
    [Preserve(AllMembers = true)]
    public class UserProfile
    {
        public int id { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string country { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string aboutYourself { get; set; }

        public bool isVerified { get; set; }
        public string avatarUrl { get; set; }

    }
}