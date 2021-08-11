namespace CapstoneAPI.Features.User
{
    using CapstoneAPI.Features.User.DataSet;
    using CapstoneAPI.Features.User.Service;
    using CapstoneAPI.Filters;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Wrappers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [Route("api/v1/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        public UsersController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("auth/google")]
        public async Task<ActionResult<Response<LoginResponse>>> LoginGoogle([FromBody] Token firebaseToken)
        {
            return Ok(await _service.Login(firebaseToken));
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet()]
        public async Task<ActionResult<PagedResponse<List<UserDataSet>>>> GetUsers([FromQuery] PaginationFilter paging,
            [FromQuery] AdminUserFilter query)
        {
            return Ok(await _service.GetListUsers(paging, query));
        }

        [HttpGet("validation")]
        public async Task<ActionResult<UserDataSet>> ValidateToken()
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.ValidateJwtToken(token));
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut()]
        public async Task<ActionResult<Response<bool>>> UpdateUser([FromBody] UpdateUserParam param)
        {
            return Ok(await _service.UpdateUser(param));
        }

        [HttpPost("unsubscribe")]
        public async Task<ActionResult<Response<bool>>> UnsubscribeTopic([FromBody] RegisterToken registerToken)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.UnsubscribeTopic(registerToken, token));
        }

        [HttpPost("subscribe")]
        public async Task<ActionResult<Response<bool>>> SubscribeTopic([FromBody] RegisterToken registerToken)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.SubscribeTopic(registerToken, token));
        }
    }
}
