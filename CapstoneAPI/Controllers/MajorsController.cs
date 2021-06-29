using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Major;
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
        [HttpGet("subject-weight")]
        public async Task<ActionResult<Response<IEnumerable<MajorSubjectWeightDataSet>>>> GetMajorWeightNumber(string majorName)
        {
            return Ok(await _service.GetMajorSubjectWeights(majorName));
        }
        [HttpGet("major-to-university")]
        public async Task<ActionResult<PagedResponse<List<MajorToUniversityDataSet>>>> GetUniversityInMajor([FromQuery] PaginationFilter filter,
            [FromQuery] MajorToUniversityFilter majorToUniversityFilter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            return Ok(await _service.GetUniversitiesInMajor(validFilter, majorToUniversityFilter));
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
    }
}
