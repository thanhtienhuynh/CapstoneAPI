using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.User.Repository
{
    public class UserRepository : GenericRepository<Models.User>, IUserRepository
    {
        public UserRepository(CapstoneDBContext context) : base(context) { }

        public async Task<Models.User> GetUserByToken(string token)
        {
            if (token == null || token.Trim().Length == 0)
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var tokenString = handler.ReadToken(token.Substring(7)) as JwtSecurityToken;
            var tokenUserId = tokenString.Claims.FirstOrDefault(claim => claim.Type == "userId").Value;
            var tokenRoleId = tokenString.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value;
            var tokenUsername = tokenString.Claims.FirstOrDefault(claim => claim.Type == "userName").Value;

            if (string.IsNullOrEmpty(tokenUserId) || !int.TryParse(tokenUserId, out _))
            {
                return null;
            }

            if (string.IsNullOrEmpty(tokenRoleId) || !int.TryParse(tokenRoleId, out _))
            {
                return null;
            }

            if (string.IsNullOrEmpty(tokenUsername))
            {
                return null;
            }

            int userId = int.Parse(tokenUserId);
            int roleId = int.Parse(tokenRoleId);
            Models.User user = await GetFirst(u => u.Id == userId && u.RoleId == roleId
                                            && u.Username == tokenUsername && u.IsActive == true);

            return user;
        }
    }
}
