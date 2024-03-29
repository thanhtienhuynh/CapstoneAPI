﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CapstoneAPI.Wrappers;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;
using CapstoneAPI.Features.SubjectGroup.Service;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneAPI.Features.SubjectGroup
{
    [Route("api/v1/subject-group")]
    [ApiController]
    public class SubjectGroupsController : Controller
    {
        private readonly ISubjectGroupService _service;
        private readonly ILogger _log = Log.ForContext<SubjectGroupsController>();

        public SubjectGroupsController(ISubjectGroupService service)
        {
            _service = service;
        }

        [HttpPost("top-subject-group")]
        public async Task<ActionResult<Response<IEnumerable<SubjectGroupDataSet>>>> SuggestTopSubjectGroup(SubjectGroupParam subjectGroupParam)
        {
            string token = Request.Headers["Authorization"];
            //var cookieToken = HttpContext.Request.Cookies["key-token"];
            //if (string.IsNullOrEmpty(cookieToken))
            //{
            //    Guid guid = Guid.NewGuid();
            //    cookieToken = guid.ToString();
            //    HttpContext.Response.Cookies.Append("key-token", guid.ToString(),
            //        new CookieOptions
            //        {
            //            MaxAge = TimeSpan.FromDays(30),
            //            HttpOnly = false,
            //            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
            //            Secure = true,
            //            IsEssential = true
            //        });
            //}
            //using (LogContext.PushProperty("cookie", true))
            //{
            //    _log.Information("User: " + cookieToken);
            //}

            return Ok(await _service.GetCaculatedSubjectGroup(subjectGroupParam, token));
        }

        [Authorize(Roles = Roles.Student)]
        [HttpGet("top-subject-group/{subjectGroupId}")]
        public async Task<ActionResult<Response<IEnumerable<SubjectGroupDataSet>>>> SuggestMajorBasedOnSubjectGroup(int subjectGroupId)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetCaculatedMajorByMockTestAndSubjectGroup(subjectGroupId, token));
        }

        [HttpGet("top-subject-group")]
        public async Task<ActionResult<Response<IEnumerable<UserSuggestionInformation>>>> GetSuggestTopSubjectGroup()
        {
            string token = Request.Headers["Authorization"];

            return Ok(await _service.GetUserSuggestTopSubjectGroup(token));
        }

        [HttpGet()]
        public async Task<ActionResult<Response<IEnumerable<AdminSubjectGroupDataSet>>>> GetSubjectGroups()
        {
            Response<IEnumerable<AdminSubjectGroupDataSet>> response = await _service.GetListSubjectGroups();
            return Ok(response);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost]
        public async Task<ActionResult<Response<CreateSubjectGroupDataset>>> CreateASubjectGroup([FromBody]CreateSubjectGroupParam createSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = await _service.CreateNewSubjectGroup(createSubjectGroupParam);
            return Ok(response);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut]
        public async Task<ActionResult<Response<CreateSubjectGroupDataset>>> UpdateASubjectGroup([FromBody]UpdateSubjectGroupParam updateSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = await _service.UpdateSubjectGroup(updateSubjectGroupParam);
            return Ok(response);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Response<SubjectGroupResponseDataSet>>> GetSubjectGroupWeight(int id)
        {
            Response<SubjectGroupResponseDataSet> response = await _service.GetSubjectGroupWeight(id);
            return Ok(response);
        }

        [Authorize(Roles = Roles.Student)]
        [HttpGet("{id}/spectrum")]
        public async Task<ActionResult<Response<List<int>>>> GetSubjectGroupSpectrum(int id)
        {
            Response<List<int>> response = await _service.GetScoreSpectrum(id);
            return Ok(response);
        }
    }
}
