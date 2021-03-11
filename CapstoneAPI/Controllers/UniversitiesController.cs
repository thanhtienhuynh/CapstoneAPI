﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CapstoneAPI.Models;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Services.University;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/university")]
    [ApiController]
    public class UniversitiesController : ControllerBase
    {

        private readonly IUniversityService _service;

        public UniversitiesController(IUniversityService service)
        {
            _service = service;
        }

        [HttpGet("suggestion")]
        public async Task<ActionResult<IEnumerable<UniversityDataSet>>> GetUniversityBySubjectGroupAndMajor([FromQuery] UniversityParam universityParam)
        {
            IEnumerable<UniversityDataSet> result = await _service.GetUniversityBySubjectGroupAndMajor(universityParam);
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<University>>> GetAllUniversities()
        {
            IEnumerable<University> result = await _service.GetUniversities();
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<ActionResult<DetailUniversityDataSet>> GetDetailUniversity([FromRoute] int id)
        {
            DetailUniversityDataSet result = await _service.GetDetailUniversity(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

    }
}
