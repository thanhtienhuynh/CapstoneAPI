using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.Helpers;
using CapstoneAPI.DataSets;
using CapstoneAPI.Wrappers;
using CapstoneAPI.Features.TestSubmission.Service;
using CapstoneAPI.Features.TestSubmission.DataSet;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneAPI.Features.TestSubmission
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

        [Authorize(Roles = Roles.Student)]
        [HttpPost("saving")]
        public async Task<ActionResult<Response<bool>>> SaveTestSubmission(List<SaveTestSubmissionParam> saveTestSubmissionParams)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.SaveTestSubmissions(saveTestSubmissionParams, token));
        }

        [Authorize(Roles = Roles.Student)]
        [HttpPost("first-saving")]
        public async Task<ActionResult<Response<int>>> SaveFirstTestSubmission(FirstTestSubmissionParam saveTestSubmissionParam)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.SaveFirstTestSubmission(saveTestSubmissionParam, token));
        }

        [Authorize(Roles = Roles.Student)]
        [HttpGet()]
        public async Task<ActionResult<Response<List<UserTestSubmissionDataSet>>>> GetTestSubmissionsByUser([FromQuery] UserTestSubmissionQueryParam param)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetTestSubmissionsByUser(token, param));
        }

        [Authorize(Roles = Roles.Student)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Response<DetailTestSubmissionDataSet>>> GetDetailTestSubmissionByUser(int id)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetDetailTestSubmissionByUser(id, token));
        }

        [HttpGet("result")]
        public async Task<ActionResult<IEnumerable<QuestionDataSet>>> Get([FromQuery] int id)
        {
            IEnumerable<QuestionDataSet> result = await _service.ScoringTest1(id);
            return Ok(result);
        }
    }
}
