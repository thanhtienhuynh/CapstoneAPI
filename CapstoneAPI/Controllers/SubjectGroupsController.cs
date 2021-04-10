using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<AdminSubjectGroupDataSet>>> GetSubjectGroupsByAdmin()
        {
            IEnumerable<AdminSubjectGroupDataSet> subjectGroups = await _service.GetListSubjectGroups();
            if (!subjectGroups.Any())
            {
                return NotFound();
            }
            return Ok(subjectGroups);
        }
        [HttpPost]
        public async Task<ActionResult<CreateSubjectGroupDataset>> CreateASubjectGroup([FromBody]CreateSubjectGroupParam createSubjectGroupParam)
        {
            CreateSubjectGroupDataset createSubjectGroupDataset = await _service.CreateNewSubjectGroup(createSubjectGroupParam);
           if(createSubjectGroupDataset == null)
            {
                return BadRequest();
            }
            return Ok(createSubjectGroupDataset);
        }
        [HttpPut]
        public async Task<ActionResult<CreateSubjectGroupDataset>> UpdateASubjectGroup([FromBody]UpdateSubjectGroupParam updateSubjectGroupParam)
        {
            CreateSubjectGroupDataset resultDataset = await _service.UpdateSubjectGroup(updateSubjectGroupParam);
            if(resultDataset == null)
            {
                return BadRequest();
            }
            return Ok(resultDataset);
        }
    }
}
