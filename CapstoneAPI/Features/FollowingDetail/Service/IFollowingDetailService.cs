using CapstoneAPI.DataSets;
using CapstoneAPI.Features.FollowingDetail.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.FollowingDetail.Service
{
    public interface IFollowingDetailService
    {
        Task<Response<Models.FollowingDetail>> AddFollowingDetail(AddFollowingDetailParam userMajorDetailParam, string token);
        Task<Response<bool>> RemoveFollowingDetail(int id, string token);
        Task<Response<IEnumerable<FollowingDetailGroupByMajorDataSet>>> GetFollowingDetailGroupByMajorDataSets(string token);
        Task<Response<IEnumerable<FollowingDetailGroupByUniversityDataSet>>> GetFollowingDetailGroupByUniversityDataSets(string token);
        Task<Response<UserFollowingDetail>> GetFollowingDetailById(int id, string token);
    }
}
