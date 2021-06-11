using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Season;
using CapstoneAPI.Services.Season;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/season")]
    [ApiController]
    public class SeasonsController : Controller
    {
        private readonly ISeasonService _service;
        public SeasonsController(ISeasonService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<AdminSeasonDataSet>>>> GetSeasons()
        {
            return Ok(await _service.GetAllSeasons());
        }
    }
}
