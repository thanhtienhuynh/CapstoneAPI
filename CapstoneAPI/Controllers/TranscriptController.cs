using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.Transcript;
using CapstoneAPI.Services.Transcript;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/transcript")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly ITranscriptService _service;
        public TranscriptController(ITranscriptService service)
        {
            _service = service;
        }
        public class Token
        {
            public string uidToken { get; set; }
        }
        [HttpGet()]
        public async Task<ActionResult<Response<IEnumerable<UserTranscriptTypeDataSet>>>> GetMarkOfUser()
        {
            string token = Request.Headers["Authorization"];
            Response<IEnumerable<UserTranscriptTypeDataSet>> result = await _service.GetMarkOfUser(token);
            return Ok(result);
        }
    }
}
