using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Services.University;
using CapstoneAPI.Wrappers;

namespace CapstoneAPI.Controllers
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

        [HttpGet()]
        public async Task<ActionResult<Response<IEnumerable<AdminUniversityDataSet>>>> GetAllUniversities()
        {
            Response<IEnumerable<AdminUniversityDataSet>> result = await _service.GetUniversities();
            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<Response<DetailUniversityDataSet>>> GetDetailUniversity([FromRoute] int id)
        {
            Response<DetailUniversityDataSet> result = await _service.GetDetailUniversity(id);
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<Response<AdminUniversityDataSet>>> CreateAnUniversity([FromBody] CreateUniversityDataset createUniversityDataset)
        {
            Response<AdminUniversityDataSet> result = await _service.CreateNewAnUniversity(createUniversityDataset);
            return Ok(result);
        }
        [HttpPut]
        public async Task<ActionResult<Response<AdminUniversityDataSet>>> UpdateUniversity([FromForm] AdminUniversityDataSet adminUniversityDataSet)
        {
            Response<AdminUniversityDataSet> result = await _service.UpdateUniversity(adminUniversityDataSet);
            return Ok(result);
        }
        [HttpPost("major-addition")]
        public async Task<ActionResult<Response<bool>>> AddMajorToUniversity([FromBody] AddingMajorUniversityParam addingMajorUniversityParam)
        {
            Response<bool> result = await _service.AddMajorToUniversity(addingMajorUniversityParam);
            return Ok(result);
        }
        [HttpPut("major-updation")]
        public async Task<ActionResult<Response<bool>>> UpdateMajorOfUniversity([FromBody] UpdatingMajorUniversityParam updatingMajorUniversityParam)
        {
            Response<bool> result = await _service.UpdateMajorOfUniversity(updatingMajorUniversityParam);
            return Ok(result);
        }

    }
}
