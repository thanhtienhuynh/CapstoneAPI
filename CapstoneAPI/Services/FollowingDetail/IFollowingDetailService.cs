using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.FollowingDetail;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.FollowingDetail
{
    public interface IFollowingDetailService
    {
        Task<Response<Models.FollowingDetail>> AddFollowingDetail(AddFollowingDetailParam userMajorDetailParam, string token);
        Task<Response<bool>> RemoveFollowingDetail(int id, string token);
        Task<Response<IEnumerable<FollowingDetailGroupByMajorDataSet>>> GetFollowingDetailGroupByMajorDataSets(string token);
        Task<Response<IEnumerable<FollowingDetailGroupByUniversityDataSet>>> GetFollowingDetailGroupByUniversityDataSets(string token);
        Task<Response<IEnumerable<RankingUserInformationGroupByRankType>>> GetUsersByFollowingDetailId(int id);
    }
}
