using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Models
{
    public class RegisterRequest
    {
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string country { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string password { get; set; }
        public string phoneNumber { get; set; } 
        public string address { get; set; }
        public string aboutYourself { get; set; }
    }
}