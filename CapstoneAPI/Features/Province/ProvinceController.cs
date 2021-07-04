using CapstoneAPI.Features.Province.DataSet;
using CapstoneAPI.Features.Province.Service;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Province
{
    [Route("api/v1/province")]
    [ApiController]
    public class ProvinceController : ControllerBase
    {
        private readonly IProvinceService _service;

        public ProvinceController(IProvinceService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<ProvinceDataSet>>>> GetProvinces()
        {
            return Ok(await _service.GetAllProvinces());
        }
    }
}
