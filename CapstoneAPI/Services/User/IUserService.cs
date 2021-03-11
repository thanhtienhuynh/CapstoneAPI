using CapstoneAPI.DataSets.User;
using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using static CapstoneAPI.Controllers.UsersController;

namespace CapstoneAPI.Services.User
{
    public interface IUserService
    {
        Task<LoginResponse> Login(Token firebaseToken);
    }
}
