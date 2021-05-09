using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CapstoneAPI.Services.SubjectGroup;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.Wrappers;

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
        public async Task<ActionResult<Response<IEnumerable<SubjectGroupDataSet>>>> SuggestTopSubjectGroup(SubjectGroupParam subjectGroupParam)
        {
            return Ok(await _service.GetCaculatedSubjectGroup(subjectGroupParam));
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
