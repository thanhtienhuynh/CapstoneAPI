﻿using System;
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
using CapstoneAPI.Filters.Test;
using CapstoneAPI.Filters;

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

        [HttpGet("test-by-subject")]
        public async Task<ActionResult<PagedResponse<List<TestPagingDataSet>>>> GetTestsByFilter([FromQuery] PaginationFilter filter,
            [FromQuery] TestFilter testFilter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            PagedResponse<List<TestPagingDataSet>> tests = await _service.GetTestsByFilter(validFilter, testFilter);
            if (tests == null)
                return NoContent();
            return Ok(tests);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Response<TestDataSet>>> GetTestById(int id)
        {
            return Ok(await _service.GetTestById(id));
        }

        [HttpPost]
        public async Task<ActionResult<Response<bool>>> AddNewTest([FromBody] NewTestParam testParam)
        {
            string token = Request.Headers["Authorization"];

            return Ok(await _service.AddNewTest(testParam, token));
        }

        [HttpPut]
        public async Task<ActionResult<Response<bool>>> UpdateTestImage()
        {
            Response<bool> result = await _service.UpdateTestImage();
            return Ok(result);
        }
    }
}
