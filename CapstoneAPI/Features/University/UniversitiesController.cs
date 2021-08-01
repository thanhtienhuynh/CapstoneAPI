using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CapstoneAPI.Wrappers;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.University;
using CapstoneAPI.Filters.MajorDetail;
using CapstoneAPI.Features.University.DataSet;
using CapstoneAPI.Features.University.Service;
using CapstoneAPI.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace CapstoneAPI.Features.University
{
    [Route("api/v1/university")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {
        private readonly IUniversityService _service;

        public UniversitiesController(IUniversityService service)
        {
            _service = service;
        }

        [HttpGet("suggestion")]
        public async Task<ActionResult<Response<IEnumerable<TrainingProgramBasedUniversityDataSet>>>> GetUniversityBySubjectGroupAndMajor([FromQuery] UniversityParam universityParam)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetUniversityBySubjectGroupAndMajor(universityParam, token));
        }

        [Authorize(Roles = Roles.Student)]
        [HttpPost("suggestion")]
        public async Task<ActionResult<Response<MockTestBasedUniversity>>> CalculattUniversityByMockTestMarks([FromBody] MockTestsUniversityParam universityParam)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.CalculaterUniversityByMockTestMarks(universityParam, token));
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet("admin-all")]
        public async Task<ActionResult<PagedResponse<List<AdminUniversityDataSet>>>> GetAllUniversities([FromQuery] PaginationFilter filter,
            [FromQuery] UniversityFilter universityFilter)
        {
            string token = Request.Headers["Authorization"];
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            PagedResponse<List<AdminUniversityDataSet>> universities = await _service.GetUniversities(validFilter, universityFilter);

            if (universities == null)
                return NoContent();
            return Ok(universities);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet("admin-non-paging")]
        public async Task<ActionResult<Response<IEnumerable<AdminUniversityDataSet>>>> GetAllUniversitiesWithOutPaging()
        {
            string token = Request.Headers["Authorization"];            
            Response<IEnumerable<AdminUniversityDataSet>> universities = await _service.GetAllUniversities();

            if (universities == null)
                return NoContent();
            return Ok(universities);
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<Response<DetailUniversityDataSet>>> GetDetailOfUniversityById(int id)
        {
            Response<DetailUniversityDataSet> result = await _service.GetDetailUniversity(id);
            return Ok(result);
        }

        [HttpGet("major-detail")]
        public async Task<ActionResult<PagedResponse<List<UniMajorDataSet>>>> GetMajorDetailInUniversity([FromQuery] PaginationFilter filter,
            [FromQuery] MajorDetailFilter majorDetailFilter)
        {
            string token = Request.Headers["Authorization"];

            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            PagedResponse<List<UniMajorDataSet>> result = await _service.GetMajorDetailInUniversity(validFilter, majorDetailFilter);
            return Ok(result);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpGet("major-detail-non-paging")]
        public async Task<ActionResult<Response<UniMajorDataSet>>> GetMajorDetailWithOutPaging([FromQuery] MajorDetailParam majorDetailParam)
        {
            Response<List<UniMajorNonPagingDataSet>> result = await _service.GetMajorDetailInUniversityNonPaging(majorDetailParam);
            return Ok(result);
        }


        [Authorize(Roles = Roles.Staff)]
        [HttpPost]
        public async Task<ActionResult<Response<AdminUniversityDataSet>>> CreateAnUniversity([FromForm] CreateUniversityDataset createUniversityDataset)
        {
            Response<AdminUniversityDataSet> result = await _service.CreateNewAnUniversity(createUniversityDataset);
            return Ok(result);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut]
        public async Task<ActionResult<Response<AdminUniversityDataSet>>> UpdateUniversity([FromForm] AdminUniversityDataSet adminUniversityDataSet)
        {
            Response<AdminUniversityDataSet> result = await _service.UpdateUniversity(adminUniversityDataSet);
            return Ok(result);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost("major-addition")]
        public async Task<ActionResult<Response<bool>>> AddMajorToUniversity([FromBody] AddingMajorUniversityParam addingMajorUniversityParam)
        {
            Response<bool> result = await _service.AddMajorToUniversity(addingMajorUniversityParam);
            return Ok(result);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut("major-updation")]
        public async Task<ActionResult<Response<bool>>> UpdateMajorOfUniversity([FromBody] UpdatingMajorUniversityParam updatingMajorUniversityParam)
        {
            Response<bool> result = await _service.UpdateMajorOfUniversity(updatingMajorUniversityParam);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<ActionResult<PagedResponse<List<AdminUniversityDataSet>>>> GetAllUniversitiesForStudents([FromQuery] PaginationFilter filter,
                                                                                                        [FromQuery] UniversityFilterForStudent universityFilter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            PagedResponse<List<AdminUniversityDataSet>> universities = await _service.GetAllUniversitiesForStudents(validFilter, universityFilter);

            if (universities == null)
                return NoContent();
            return Ok(universities);
        }

    }
}
