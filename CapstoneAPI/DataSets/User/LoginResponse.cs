using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.User
{
    public class LoginResponse
    {
        public bool IsAdmin { get; set; }
        public string Token { get; set; }
    }
}
