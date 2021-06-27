using CapstoneAPI.DataSets.MajorSubjectGroup;
using CapstoneAPI.Services.MajorSubjectGroup;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Controllers
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


        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<MajorSubjectGroupDataSet>>>> GetMajorSubjectGroupByGroup([FromQuery]int MajorId)
        {
            return Ok(await _service.GetMajorSubjectGourpByMajor(MajorId));
        }

        [HttpPost]
        public async Task<ActionResult<Response<MajorSubjectGroupDataSet>>> AddAMajorSubjectGroup([FromBody] MajorSubjectGroupParam majorSubjectGroupParam)
        {
            return Ok(await _service.AddAMajorSubjectGroup(majorSubjectGroupParam));
        }
    }
}
