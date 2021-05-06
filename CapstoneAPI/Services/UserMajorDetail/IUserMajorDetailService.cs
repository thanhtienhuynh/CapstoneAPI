using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.UserMajorDetail;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.UserMajorDetail
{
    public interface IUserMajorDetailService
    {
        Task<Response<Models.UserMajorDetail>> AddUserMajorDetail(AddUserMajorDetailParam userMajorDetailParam, string token);
        Task<Response<Object>> RemoveUserMajorDetail(UpdateUserMajorDetailParam userMajorDetailParam, string token);
    }
}
