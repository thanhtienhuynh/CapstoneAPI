using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/logger")]
    [ApiController]
    public class LoggerController : Controller
    {
        private readonly ILogger _log = Log.ForContext<LoggerController>();

        public LoggerController()
        {
        }

        public class ErrorMessage
        {
            public string Message { get; set; }
        }

        [HttpPost()]
        public async Task<ActionResult<Object>> WriteLog([FromBody] ErrorMessage error)
        {
            using (LogContext.PushProperty("fe", true))
            {
                _log.Information(error.Message);
            }
            return Ok();
        }
    }
}
