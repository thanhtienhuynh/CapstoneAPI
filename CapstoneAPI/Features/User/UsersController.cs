namespace CapstoneAPI.Features.User
{
    using CapstoneAPI.Features.User.DataSet;
    using CapstoneAPI.Features.User.Service;
    using CapstoneAPI.Wrappers;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
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

        [AllowAnonymous]
        [HttpPost("auth/google")]
        public async Task<ActionResult<Response<LoginResponse>>> LoginGoogle([FromBody] Token firebaseToken)
        {
            return Ok(await _service.Login(firebaseToken));
        }
    }
}
