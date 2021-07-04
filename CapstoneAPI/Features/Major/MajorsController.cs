using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Features.Major.DataSet;
using CapstoneAPI.Features.Major.Service;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Major;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Features.Major
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

        [HttpGet("student-all")]
        public async Task<ActionResult<PagedResponse<List<NumberUniversityInMajorDataSet>>>> GetNumberUniversityInMajor([FromQuery] PaginationFilter filter,
            [FromQuery] MajorToNumberUniversityFilter majorToUniversityFilter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            return Ok(await _service.GetNumberUniversitiesInMajor(validFilter, majorToUniversityFilter));
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

        [HttpPut("subject-weight2")]
        public async Task<ActionResult<Response<UpdateMajorParam>>> UpdateMajor([FromBody] UpdateMajorParam2 updateMajor)
        {
            return Ok(await _service.UpdateMajor(updateMajor));
        }

        [HttpGet("student-detail/{id}")]
        public async Task<ActionResult<Response<MajorDetailDataSet>>> GetUniversityInMajor([FromRoute]int id)
        {
            return Ok(await _service.GetUniversitiesInMajor(id));
        }
    }
}
