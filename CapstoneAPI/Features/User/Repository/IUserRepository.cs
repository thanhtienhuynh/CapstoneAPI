using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.User.Repository
{
    public interface IUserRepository : IGenericRepository<Models.User>
    {
        Task<Models.User> GetUserByToken(string token) ;
    }
}
