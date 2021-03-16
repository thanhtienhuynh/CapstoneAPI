using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Services.University;

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
        public async Task<ActionResult<IEnumerable<UniversityDataSet>>> GetUniversityBySubjectGroupAndMajor([FromQuery] UniversityParam universityParam)
        {
            IEnumerable<UniversityDataSet> result = await _service.GetUniversityBySubjectGroupAndMajor(universityParam);
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<AdminUniversityDataSet>>> GetAllUniversities()
        {
            IEnumerable<AdminUniversityDataSet> result = await _service.GetUniversities();
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<DetailUniversityDataSet>> GetDetailUniversity([FromRoute] int id)
        {
            DetailUniversityDataSet result = await _service.GetDetailUniversity(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpPost]
        public async Task<ActionResult<AdminUniversityDataSet>> CreateAnUniversity([FromBody] CreateUniversityDataset createUniversityDataset)
        {
            AdminUniversityDataSet result = await _service.CreateNewAnUniversity(createUniversityDataset);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }
        [HttpPut]
        public async Task<ActionResult<AdminUniversityDataSet>> UpdateUniversity([FromBody] AdminUniversityDataSet adminUniversityDataSet)
        {
            AdminUniversityDataSet result = await _service.UpdateUniversity(adminUniversityDataSet);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }
        [HttpPost("major-addition")]
        public async Task<ActionResult<DetailUniversityDataSet>> AddMajorToUniversity([FromBody] AddingMajorUniversityParam addingMajorUniversityParam)
        {
            DetailUniversityDataSet result = await _service.AddMajorToUniversity(addingMajorUniversityParam);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }
        [HttpPut("major-updation")]
        public async Task<ActionResult<DetailUniversityDataSet>> UpdateMajorOfUniversity([FromBody] UpdatingMajorUniversityParam updatingMajorUniversityParam)
        {
            DetailUniversityDataSet result = await _service.UpdateMajorOfUniversity(updatingMajorUniversityParam);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok(result);
        }

    }
}
