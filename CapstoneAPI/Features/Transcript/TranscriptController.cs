using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Features.Transcript.DataSet;
using CapstoneAPI.Features.Transcript.Service;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Features.Transcript
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

        [HttpPost()]
        public async Task<ActionResult<Response<bool>>> SaveMarkOfUser([FromBody] SubjectGroupParam subjectGroupParam)
        {
            string token = Request.Headers["Authorization"];
            Response<bool> result = await _service.SaveMarkOfUser(token, subjectGroupParam);
            return Ok(result);
        }
    }
}
