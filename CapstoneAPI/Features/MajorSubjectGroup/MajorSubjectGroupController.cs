using CapstoneAPI.Features.MajorSubjectGroup.DataSet;
using CapstoneAPI.Features.MajorSubjectGroup.Service;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.MajorSubjectGroup
{
    [Route("api/v1/major-subject-group")]
    [ApiController]
    public class MajorSubjectGroupController : ControllerBase
    {
        private readonly IMajorSubjectGroupService _service;

        public MajorSubjectGroupController(IMajorSubjectGroupService service)
        {
            _service = service;
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<MajorSubjectGroupDataSet>>>> GetMajorSubjectGroupByMajor([FromQuery]int MajorId)
        {
            return Ok(await _service.GetMajorSubjectGourpByMajor(MajorId));
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost]
        public async Task<ActionResult<Response<MajorSubjectGroupDataSet>>> AddAMajorSubjectGroup([FromBody] MajorSubjectGroupParam majorSubjectGroupParam)
        {
            return Ok(await _service.AddAMajorSubjectGroup(majorSubjectGroupParam));
        }
    }
}
