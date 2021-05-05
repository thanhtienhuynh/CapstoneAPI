using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.UserMajorDetail;
using CapstoneAPI.Models;
using CapstoneAPI.Services.UserMajorDetail;
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
        public async Task<ActionResult<UserMajorDetail>> AddUserMajorDetail(AddUserMajorDetailParam userMajorDetailParam)
        {
            string token = Request.Headers["Authorization"];
            UserMajorDetail userMajorDetail = await _service.AddUserMajorDetail(userMajorDetailParam, token);
            if (userMajorDetail == null)
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpPost("deletion")]
        public async Task<ActionResult<UserMajorDetail>> RemoveUserMajorDetail(UpdateUserMajorDetailParam userMajorDetailParam)
        {
            string token = Request.Headers["Authorization"];
            BaseResponse<object> reponse = await _service.RemoveUserMajorDetail(userMajorDetailParam, token);
            switch(reponse.StatusCode)
            {
                case 0:
                    return Ok(reponse);
                case 1:
                    return Unauthorized(reponse);
                case 2:
                    return NotFound(reponse);
                case 3:
                    return BadRequest(reponse);
            }
            return NoContent();
        }

        [HttpGet("group-by-major")]
        public async Task<ActionResult<IEnumerable<UserMajorDetailGroupByMajorDataSet>>> GetUserMajorDetailGroupByMajor()
        {
            string token = Request.Headers["Authorization"];
            IEnumerable<UserMajorDetailGroupByMajorDataSet> UserMajorDetailGroupByMajorDataSet = await _service.GetUserMajorDetailGroupByMajorDataSets(token);
            if (UserMajorDetailGroupByMajorDataSet == null)
            {
            return BadRequest();
            }
            return Ok(UserMajorDetailGroupByMajorDataSet);
        }
    }
}
