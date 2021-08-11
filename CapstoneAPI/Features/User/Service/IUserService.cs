using CapstoneAPI.Features.User.DataSet;
using CapstoneAPI.Filters;
using CapstoneAPI.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.User.Service
{
    public interface IUserService
    {
        Task<Response<UserDataSet>> ValidateJwtToken(string token);
        Task<Response<LoginResponse>> Login(Token firebaseToken);
        Task<PagedResponse<List<UserDataSet>>> GetListUsers(PaginationFilter paging, AdminUserFilter query);
        Task<Response<bool>> UpdateUser(UpdateUserParam param);
        Task<Response<bool>> UnsubscribeTopic(RegisterToken registerToken, string token);
        Task<Response<bool>> SubscribeTopic(RegisterToken registerToken, string token);
    }
}
