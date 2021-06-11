using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Services.Rank;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/rank")]
    [ApiController]
    public class RankController : Controller
    {
        private readonly IRankService _service;
        public RankController(IRankService service)
        {
            _service = service;
        }


        [HttpPut("updation")]
        public async Task<IActionResult> UpdateRank()
        {
            return Ok(await _service.UpdateRank());
        }
    }
}
