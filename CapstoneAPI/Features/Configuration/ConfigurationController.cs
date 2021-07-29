using CapstoneAPI.Features.Configuration.Service;
using CapstoneAPI.Wrappers;
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

        [HttpGet("pagination")]
        public Response<object> GetPaginationConfiguration()
        {
            return _service.GetPaginationConfiguration();
        }

        [HttpPut("pagination")]
        public Response<object> SetPaginationConfiguration(int? firstPage, int? highestQuantity)
        {
            return _service.SetPaginationConfiguration(firstPage, highestQuantity);
        }
    }
}
