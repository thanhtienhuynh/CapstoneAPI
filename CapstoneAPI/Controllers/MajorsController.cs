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
    }
}
