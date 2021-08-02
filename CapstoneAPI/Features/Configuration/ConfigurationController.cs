using CapstoneAPI.Features.Configuration.DataSet;
using CapstoneAPI.Features.Configuration.Service;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Configuration
{
    [Route("api/v1/configuration")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _service;
        public ConfigurationController(IConfigurationService service)
        {
            _service = service;
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet("pagination")]
        public Response<object> GetPaginationConfiguration()
        {
            return _service.GetPaginationConfiguration();
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut("pagination")]
        public Response<object> SetPaginationConfiguration(int? firstPage, int? highestQuantity)
        {
            return _service.SetPaginationConfiguration(firstPage, highestQuantity);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut("app")]
        public async Task<ActionResult<Response<ConfigParam>>> SetAppConfig([FromBody] ConfigParam configParam)
        {
            return Ok(await _service.SetAppConfiguration(configParam));
        }

        //[Authorize(Roles = Roles.Staff)]
        [HttpGet("app")]
        public async Task<ActionResult<Response<ConfigParam>>> GetAppConfig()
        {
            return Ok(await _service.GetAppConfiguration());
        }
    }
}
