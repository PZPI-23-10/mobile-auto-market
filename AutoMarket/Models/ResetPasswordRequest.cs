using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMarket.Models
{
    public class ResetPasswordRequest
    {
        public string email { get; set; }
        public string password { get; set; }
        public string passwordConfirmation { get; set; }
    }
}
