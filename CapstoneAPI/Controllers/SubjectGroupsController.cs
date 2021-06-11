using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CapstoneAPI.Services.SubjectGroup;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.Wrappers;
using System.Net.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace CapstoneAPI.Controllers
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
            var cookieToken = HttpContext.Request.Cookies["key-token"];
            if (string.IsNullOrEmpty(cookieToken))
            {
                Guid guid = Guid.NewGuid();
                cookieToken = guid.ToString();
                HttpContext.Response.Cookies.Append("key-token", guid.ToString(),
                    new CookieOptions
                    {
                        MaxAge = TimeSpan.FromDays(30),
                        HttpOnly = false,
                        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                        Secure = true,
                        IsEssential = true
                    });
            }
            using (LogContext.PushProperty("cookie", true))
            {
                _log.Information("User: " + cookieToken);
            }
           
            return Ok(await _service.GetCaculatedSubjectGroup(subjectGroupParam));
        }

        [HttpGet("top-subject-group")]
        public async Task<ActionResult<Response<IEnumerable<UserSuggestionInformation>>>> GetSuggestTopSubjectGroup()
        {
            string token = Request.Headers["Authorization"];

            return Ok(await _service.GetUserSuggestTopSubjectGroup(token));
        }

        [HttpGet()]
        public async Task<ActionResult<Response<IEnumerable<AdminSubjectGroupDataSet>>>> GetSubjectGroupsByAdmin()
        {
            Response<IEnumerable<AdminSubjectGroupDataSet>> response = await _service.GetListSubjectGroups();
            return Ok(response);
        }
        [HttpPost]
        public async Task<ActionResult<Response<CreateSubjectGroupDataset>>> CreateASubjectGroup([FromBody]CreateSubjectGroupParam createSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = await _service.CreateNewSubjectGroup(createSubjectGroupParam);
            return Ok(response);
        }
        [HttpPut]
        public async Task<ActionResult<Response<CreateSubjectGroupDataset>>> UpdateASubjectGroup([FromBody]UpdateSubjectGroupParam updateSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = await _service.UpdateSubjectGroup(updateSubjectGroupParam);
            return Ok(response);
        }
    }
}
