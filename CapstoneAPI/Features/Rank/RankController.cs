using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Features.Rank.Service;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Features.Rank
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
        public async Task<ActionResult<Response<bool>>> UpdateRank()
        {
            return Ok(await _service.UpdateRank());
        }
    }
}
