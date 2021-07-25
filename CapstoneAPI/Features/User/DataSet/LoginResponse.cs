using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.User.DataSet
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public UserDataSet User { get; set; }
    }

    public class Token
    {
        public string uidToken { get; set; }
    }

    public class RegisterToken
    {
        public string token { get; set; }
    }
}
