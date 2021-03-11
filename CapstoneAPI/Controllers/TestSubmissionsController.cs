using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.Services.TestSubmission;
using CapstoneAPI.DataSets.TestSubmission;
using CapstoneAPI.DataSets.Question;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/test-submission")]
    [ApiController]
    public class TestSubmissionsController : ControllerBase
    {
        private readonly ITestSubmissionService _service;

        public TestSubmissionsController(ITestSubmissionService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<ActionResult<TestSubmissionDataSet>> ScoringTest(TestSubmissionParam testSubmissionParam)
        {
            TestSubmissionDataSet result = await _service.ScoringTest(testSubmissionParam);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionDataSet>>> Get()
        {
            IEnumerable<QuestionDataSet> result = await _service.ScoringTest1();
            return Ok(result);
        }
    }
}
