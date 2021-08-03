using CapstoneAPI.DataSets;
using CapstoneAPI.Features.Season.DataSet;
using CapstoneAPI.Features.Season.Service;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Season
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
        public async Task<ActionResult<Response<List<AdminSeasonDataSet>>>> GetSeasons()
        {
            return Ok(await _service.GetAllSeasons());
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost]
        public async Task<ActionResult<Response<AdminSeasonDataSet>>> CreateSeason([FromBody] CreateSeasonParam createSeasonParam)
        {
            return Ok(await _service.CreateSeason(createSeasonParam));
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut]
        public async Task<ActionResult<Response<AdminSeasonDataSet>>> UpdateSeason([FromBody] UpdateSeasonParam updateSeasonParam)
        {
            return Ok(await _service.UpdateSeason(updateSeasonParam));
        }
    }
}
