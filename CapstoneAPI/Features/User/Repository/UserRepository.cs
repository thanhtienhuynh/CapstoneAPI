using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
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

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (string.IsNullOrEmpty(userIdString) || !Int32.TryParse(userIdString, out int userId))
            {
                return null;
            }

            userId = Int32.Parse(userIdString);

            Models.User user = await GetById(userId);

            return user;
        }
    }
}
