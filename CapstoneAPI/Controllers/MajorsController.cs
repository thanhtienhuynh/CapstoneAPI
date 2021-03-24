using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Services.Major;
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
        public async Task<ActionResult<IEnumerable<AdminMajorDataSet>>> GetMajorsByAdmin()
        {
            IEnumerable<AdminMajorDataSet> majors = await _service.GetActiveMajorsByAdmin();
            if (!majors.Any())
            {
                return NotFound();
            }
            return Ok(majors);
        }

        [HttpPost]
        public async Task<ActionResult<ResultOfCreateMajorDataSet>> CreateAMajor([FromBody] CreateMajorDataSet createMajorDataSet)
        {
            ResultOfCreateMajorDataSet newMajor = await _service.CreateAMajor(createMajorDataSet);

            if(newMajor == null)
            {
                return BadRequest();
            }
            return Ok(newMajor);
        }
        [HttpPut]
        public async Task<ActionResult<ResultOfCreateMajorDataSet>> UpdateMajor([FromBody] ResultOfCreateMajorDataSet updateMajorDataSet)
        {
            ResultOfCreateMajorDataSet updatedMajor = await _service.UpdateAMajor(updateMajorDataSet);

            if (updatedMajor == null)
            {
                return BadRequest();
            }
            return Ok(updatedMajor);
        }
    }
}
