namespace CapstoneAPI.Controllers
{
    using CapstoneAPI.DataSets.User;
    using CapstoneAPI.Services.User;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.IdentityModel.Tokens.Jwt;
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
        public class Token
        {
            public string uidToken { get; set; }
        }

        [AllowAnonymous]
        [HttpPost("auth/google")]
        public async Task<ActionResult<LoginResponse>> LoginGoogle([FromBody] Token firebaseToken)
        {
            LoginResponse result = await _service.Login(firebaseToken);
            if (result != null)
            {
                return Ok(result);
            } else
            {
                return BadRequest();
            }
        }
    }
}
