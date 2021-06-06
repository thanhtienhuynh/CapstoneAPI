using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.FollowingDetail;
using CapstoneAPI.Models;
using CapstoneAPI.Services.FollowingDetail;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/following-detail")]
    [ApiController]
    public class FollowingDetailController : Controller
    {
        private readonly IFollowingDetailService _service;

        public FollowingDetailController(IFollowingDetailService service)
        {
            _service = service;
        }

        [HttpPost()]
        public async Task<ActionResult<Response<FollowingDetail>>> AddFollowingDetail(AddFollowingDetailParam addFollowingDetailParam)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.AddFollowingDetail(addFollowingDetailParam, token));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<bool>>> RemoveUserMajorDetail([FromRoute] int id)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.RemoveFollowingDetail(id, token));
        }

        [HttpGet("group-by-major")]
        public async Task<ActionResult<Response<IEnumerable<FollowingDetailGroupByMajorDataSet>>>> GetFollowingDetailGroupByMajor()
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetFollowingDetailGroupByMajorDataSets(token));
        }

        [HttpGet("group-by-university")]
        public async Task<ActionResult<Response<IEnumerable<FollowingDetailGroupByUniversityDataSet>>>> GetFollowingDetailGroupByUniversity()
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetFollowingDetailGroupByUniversityDataSets(token));
        }

        [HttpGet("users-group-by-major-detail/{id}")]
        public async Task<ActionResult<Response<IEnumerable<RankingUserInformationGroupByRankType>>>> GetUsersByFollowingDetailId([FromRoute] int id)
        {
            return Ok(await _service.GetUsersByFollowingDetailId(id));
        }
    }
}
