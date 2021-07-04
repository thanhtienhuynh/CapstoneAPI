using CapstoneAPI.Features.User.DataSet;
using CapstoneAPI.Wrappers;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.User.Service
{
    public interface IUserService
    {
        Task<Response<LoginResponse>> Login(Token firebaseToken);
    }
}
