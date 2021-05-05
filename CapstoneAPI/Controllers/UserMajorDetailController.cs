using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.UserMajorDetail;
using CapstoneAPI.Models;
using CapstoneAPI.Services.UserMajorDetail;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/user-major-detail")]
    [ApiController]
    public class UserMajorDetailController : Controller
    {
        private readonly IUserMajorDetailService _service;

        public UserMajorDetailController(IUserMajorDetailService service)
        {
            _service = service;
        }

        [HttpPost()]
        public async Task<ActionResult<Response<UserMajorDetail>>> AddUserMajorDetail(AddUserMajorDetailParam userMajorDetailParam)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.AddUserMajorDetail(userMajorDetailParam, token));
        }

        [HttpPost("deletion")]
        public async Task<ActionResult<Response<Object>>> RemoveUserMajorDetail(UpdateUserMajorDetailParam userMajorDetailParam)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.RemoveUserMajorDetail(userMajorDetailParam, token));
        }

        [HttpGet("group-by-major")]
        public async Task<ActionResult<Response<IEnumerable<UserMajorDetailGroupByMajorDataSet>>>> GetUserMajorDetailGroupByMajor()
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetUserMajorDetailGroupByMajorDataSets(token));
        }

        [HttpGet("group-by-university")]
        public async Task<ActionResult<Response<IEnumerable<UserMajorDetailGroupByUniversityDataSet>>>> GetUserMajorDetailGroupByUniversity()
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetUserMajorDetailGroupByUniversityDataSets(token));
        }
    }
}
