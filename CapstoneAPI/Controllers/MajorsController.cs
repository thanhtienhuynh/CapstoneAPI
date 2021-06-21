using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Filters;
using CapstoneAPI.Services.Major;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/major")]
    [ApiController]
    public class MajorsController : Controller
    {
        private readonly IMajorService _service;
        public MajorsController(IMajorService service)
        {
            _service = service;
        }

        [HttpGet()]
        public async Task<ActionResult<Response<IEnumerable<AdminMajorDataSet>>>> GetMajorsByAdmin()
        {
            return Ok(await _service.GetActiveMajorsByAdmin());
        }
        [HttpGet("subject-weight-non-paging")]
        public async Task<ActionResult<Response<List<MajorSubjectWeightDataSet>>>> GetMajorWeightNumber(string majorName)
        {
            return Ok(await _service.GetMajorSubjectWeights(majorName));
        }
        [HttpGet("subject-weight")]
        public async Task<ActionResult<Response<List<MajorSubjectWeightDataSet>>>> GetMajorWeightNumber([FromQuery] PaginationFilter validFilter, string majorName)
        {
            return Ok(await _service.GetMajorSubjectWeights(validFilter, majorName));
        }
        [HttpPost]
        public async Task<ActionResult<Response<ResultOfCreateMajorDataSet>>> CreateAMajor([FromBody] CreateMajorDataSet createMajorDataSet)
        {
            return Ok(await _service.CreateAMajor(createMajorDataSet));
        }
        [HttpPut]
        public async Task<ActionResult<Response<ResultOfCreateMajorDataSet>>> UpdateMajor([FromBody] ResultOfCreateMajorDataSet updateMajorDataSet)
        {
            return Ok(await _service.UpdateAMajor(updateMajorDataSet));
        }
        [HttpPost("subject-weight")]
        public async Task<ActionResult<Response<CreateMajorSubjectWeightDataSet>>> CreateAMajorWeightNumber([FromBody] CreateMajorSubjectWeightDataSet createMajorDataSet)
        {
            return Ok(await _service.CreateAMajor(createMajorDataSet));
        }
        [HttpPut("subject-weight")]
        public async Task<ActionResult<Response<UpdateMajorParam>>> UpdateMajor([FromBody] UpdateMajorParam updateMajor)
        {
            return Ok(await _service.UpdateMajor(updateMajor));
        }
    }
}
