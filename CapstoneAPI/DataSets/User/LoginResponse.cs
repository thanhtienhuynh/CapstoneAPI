using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.User
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public UserDataSet User { get; set; }
    }
}
