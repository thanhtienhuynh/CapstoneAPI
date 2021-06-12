﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.TrainingProgram;
using CapstoneAPI.Services.TrainingProgram;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/trainingprogram")]
    [ApiController]
    public class TrainingProgramsController : Controller
    {
        private readonly ITrainingProgramService _service;
        public TrainingProgramsController(ITrainingProgramService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<AdminTrainingProgramDataSet>>>> GetTrainingPrograms()
        {
            Response<IEnumerable<AdminTrainingProgramDataSet>> trainingPrograms = await _service.AdminGetAllTrainingPrograms();
            return Ok(trainingPrograms);
        }

        [HttpPost]
        public async Task<ActionResult<Response<AdminTrainingProgramDataSet>>> CreateNewTrainingProgram([FromBody] CreateTrainingProgramParam createTrainingProgramParam)
        {
            Response<AdminTrainingProgramDataSet> result = await _service.CreateATrainingProgram(createTrainingProgramParam);
            return Ok(result);
        }
        [HttpPut]
        public async Task<ActionResult<Response<AdminTrainingProgramDataSet>>> UpdateATrainingProgram([FromBody] AdminTrainingProgramDataSet updateTrainingProgramParam)
        {
            Response<AdminTrainingProgramDataSet> result = await _service.UpdateATrainingProgram(updateTrainingProgramParam);
            return Ok(result);
        }
    }
}
