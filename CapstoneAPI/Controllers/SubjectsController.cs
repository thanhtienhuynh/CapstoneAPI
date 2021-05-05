using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.Services.Subject;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.Wrappers;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/subject")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _service;

        public SubjectsController(ISubjectService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<SubjectDataSet>>>> GetSubjects()
        {
            return Ok(await _service.GetAllSubjects());
        }
    }
}
