using CapstoneAPI.Features.User.DataSet;
using CapstoneAPI.Wrappers;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.User.Service
{
    public interface IUserService
    {
        Task<Response<LoginResponse>> Login(Token firebaseToken);
        Task<Response<bool>> UnsubscribeTopic(RegisterToken registerToken, string token);
        Task<Response<bool>> SubscribeTopic(RegisterToken registerToken, string token);
    }
}
