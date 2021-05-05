using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.UserMajorDetail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.UserMajorDetail
{
    public interface IUserMajorDetailService
    {
        Task<Models.UserMajorDetail> AddUserMajorDetail(AddUserMajorDetailParam userMajorDetailParam, string token);
        Task<BaseResponse<Object>> RemoveUserMajorDetail(UpdateUserMajorDetailParam userMajorDetailParam, string token);
        Task<IEnumerable<UserMajorDetailGroupByMajorDataSet>> GetUserMajorDetailGroupByMajorDataSets(string token);
    }
}
