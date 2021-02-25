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
        public async Task<ActionResult<IEnumerable<TestDataSet>>> GetFilteredTests([FromQuery]TestParam testParam )
        {
            IEnumerable<TestDataSet> results = await _service.GetFilteredTests(testParam);
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestDataSet>> GetTestById(int id)
        {
            TestDataSet result = await _service.GetTestById(id);
            return Ok(result);
        }

    }
}
