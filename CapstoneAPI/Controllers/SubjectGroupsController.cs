using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.Services.SubjectGroup;
using CapstoneAPI.DataSets.SubjectGroup;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/subject-group")]
    [ApiController]
    public class SubjectGroupsController : Controller
    {
        private readonly ISubjectGroupService _service;

        public SubjectGroupsController(ISubjectGroupService service)
        {
            _service = service;
        }

        [HttpPost("top-subject-group")]
        public async Task<ActionResult<IEnumerable<SubjectGroupDataSet>>> SuggestTopSubjectGroup(SubjectGroupParam subjectGroupParam)
        {
            IEnumerable<SubjectGroupDataSet> subjectGroups = await _service.GetCaculatedSubjectGroup(subjectGroupParam);
            return Ok(subjectGroups);
        }

    }
}
