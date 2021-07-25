using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Helpers
{
    public static class JWTUtils
    {
        public static string GetUserIdFromJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenString = handler.ReadToken(token.Substring(7)) as JwtSecurityToken;
            var userId = tokenString.Claims.First(claim => claim.Type == "sub").Value;
            return userId;
        }
    }
}
