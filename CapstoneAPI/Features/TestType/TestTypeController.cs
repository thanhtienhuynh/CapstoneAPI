using CapstoneAPI.Features.TestType.DataSet;
using CapstoneAPI.Features.TestType.Service;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.TestType
{
    [Route("api/v1/test-type")]
    [ApiController]
    public class TestTypeController : ControllerBase
    {
        private readonly ITestTypeService _service;

        public TestTypeController(ITestTypeService service)
        {
            _service = service;
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<TestTypeDataSet>>>> GetAllTestTypes()
        {
            Response<IEnumerable<TestTypeDataSet>> result = await _service.GetAllTestTypes();
            return Ok(result);
        }
    }
}
