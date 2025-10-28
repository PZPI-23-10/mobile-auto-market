using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Models
{
    public class LoginRequest
    {
        // Використовуємо маленькі літери, як у RegisterRequest
        public string email { get; set; }
        public string password { get; set; }
        public bool rememberMe { get; set; } = true;
    }
}
