using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.Services.Test;
using CapstoneAPI.DataSets.Test;
using CapstoneAPI.Wrappers;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/test")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly ITestService _service;

        public TestsController(ITestService service)
        {
            _service = service;
        }

        [HttpGet("recommendation")]
        public async Task<ActionResult<Response<List<SubjectBasedTestDataSet>>>> GetFilteredTests([FromQuery]TestParam testParam )
        {
            return Ok(await _service.GetFilteredTests(testParam));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Response<TestDataSet>>> GetTestById(int id)
        {
            return Ok(await _service.GetTestById(id));
        }

    }
}
