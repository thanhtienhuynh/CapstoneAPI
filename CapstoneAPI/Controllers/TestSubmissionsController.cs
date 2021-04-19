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

        [HttpPost("saving")]
        public async Task<ActionResult<BaseResponse<TestSubmission>>> SaveTestSubmission(SaveTestSubmissionParam saveTestSubmissionParam)
        {
            string token = Request.Headers["Authorization"];
            BaseResponse<TestSubmission> result = await _service.SaveTestSubmission(saveTestSubmissionParam, token);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet()]
        public async Task<ActionResult<List<UserTestSubmissionDataSet>>> GetTestSubmissionsByUser()
        {
            string token = Request.Headers["Authorization"];
            List<UserTestSubmissionDataSet> result = await _service.GetTestSubmissionsByUser(token);
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailTestSubmissionDataSet>> GetDetailTestSubmissionByUser(int id)
        {
            string token = Request.Headers["Authorization"];
            DetailTestSubmissionDataSet result = await _service.GetDetailTestSubmissionByUser(id, token);
            if (result == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }

        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<QuestionDataSet>>> Get()
        //{
        //    IEnumerable<QuestionDataSet> result = await _service.ScoringTest1();
        //    return Ok(result);
        //}
    }
}
