using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapstoneAPI.Features.FollowingDetail.DataSet;
using CapstoneAPI.Features.FollowingDetail.Service;
using CapstoneAPI.Models;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Features.FollowingDetail
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
        public async Task<ActionResult<Response<Models.FollowingDetail>>> AddFollowingDetail(AddFollowingDetailParam addFollowingDetailParam)
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

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<Response<UserFollowingDetail>>> GetUsersByFollowingDetailId([FromRoute] int id)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetFollowingDetailById(id, token));
        }
    }
}
