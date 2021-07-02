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
using CapstoneAPI.Helpers;
using CapstoneAPI.DataSets;
using CapstoneAPI.Wrappers;

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
        public async Task<ActionResult<Response<TestSubmissionDataSet>>> ScoringTest(TestSubmissionParam testSubmissionParam)
        {
            return Ok(await _service.ScoringTest(testSubmissionParam));
        }

        [HttpPost("saving")]
        public async Task<ActionResult<Response<bool>>> SaveTestSubmission(List<SaveTestSubmissionParam> saveTestSubmissionParams)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.SaveTestSubmissions(saveTestSubmissionParams, token));
        }

        [HttpGet()]
        public async Task<ActionResult<Response<List<UserTestSubmissionDataSet>>>> GetTestSubmissionsByUser([FromQuery] UserTestSubmissionQueryParam param)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetTestSubmissionsByUser(token, param));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Response<DetailTestSubmissionDataSet>>> GetDetailTestSubmissionByUser(int id)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetDetailTestSubmissionByUser(id, token));
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<QuestionDataSet>>> Get()
        //{
        //    IEnumerable<QuestionDataSet> result = await _service.ScoringTest1();
        //    return Ok(result);
        //}
    }
}
