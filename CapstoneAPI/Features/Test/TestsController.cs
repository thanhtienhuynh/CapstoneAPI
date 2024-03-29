﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.Wrappers;
using CapstoneAPI.Filters.Test;
using CapstoneAPI.Filters;
using CapstoneAPI.Features.Test.DataSet;
using CapstoneAPI.Features.Test.Service;
using CapstoneAPI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneAPI.Features.Test
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

        [Authorize(Roles = Roles.Student)]
        [HttpGet("recommendation")]
        public async Task<ActionResult<Response<List<SubjectBasedTestDataSet>>>> GetFilteredTests([FromQuery]TestParam testParam )
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetFilteredTests(token, testParam));
        }

        [HttpGet("user-by-subject")]
        public async Task<ActionResult<PagedResponse<List<TestPagingDataSet>>>> GetTestsByFilter([FromQuery] PaginationFilter filter,
            [FromQuery] TestFilter testFilter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            PagedResponse<List<TestPagingDataSet>> tests = await _service.GetTestsByFilter(validFilter, testFilter);
            if (tests == null)
                return NoContent();
            return Ok(tests);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet("admin-by-subject")]
        public async Task<ActionResult<PagedResponse<List<TestAdminDataSet>>>> AdminGetTestsByFilter([FromQuery] PaginationFilter filter,
            [FromQuery] TestFilter testFilter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            PagedResponse<List<TestAdminDataSet>> tests = await _service.AdminGetTestsByFilter(validFilter, testFilter);
            if (tests == null)
                return NoContent();
            return Ok(tests);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Response<TestDataSet>>> GetTestById(int id)
        {
            return Ok(await _service.GetTestById(id));
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost]
        public async Task<ActionResult<Response<bool>>> AddNewTest([FromBody] NewTestParam testParam)
        {
            string token = Request.Headers["Authorization"];

            return Ok(await _service.AddNewTest(testParam, token));
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut("system")]
        public async Task<ActionResult<Response<bool>>> UpdateTestImage()
        {
            Response<bool> result = await _service.UpdateTestImage();
            return Ok(result);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut("admin")]
        public async Task<ActionResult<Response<bool>>> UpdateTest([FromBody] UpdateTestParam testParam)
        {
            string token = Request.Headers["Authorization"];
            Response<bool> result = await _service.UpdateTest(testParam, token);
            return Ok(result);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut("admin-suggest-test")]
        public async Task<ActionResult<Response<bool>>> UpdateSuggestTest([FromBody] SetSuggestedTestParam setSuggestedTestParam)
        {
            string token = Request.Headers["Authorization"];
            Response<bool> result = await _service.UpdateSuggestTest(setSuggestedTestParam, token);
            return Ok(result);
        }
    }
}
