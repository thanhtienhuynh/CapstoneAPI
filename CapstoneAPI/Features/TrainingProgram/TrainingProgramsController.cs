using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Features.TrainingProgram.DataSet;
using CapstoneAPI.Features.TrainingProgram.Service;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Features.TrainingProgram
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

        [Authorize(Roles = Roles.Staff)]
        [HttpGet]
        public async Task<ActionResult<Response<IEnumerable<AdminTrainingProgramDataSet>>>> GetTrainingPrograms()
        {
            Response<IEnumerable<AdminTrainingProgramDataSet>> trainingPrograms = await _service.AdminGetAllTrainingPrograms();
            return Ok(trainingPrograms);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost]
        public async Task<ActionResult<Response<AdminTrainingProgramDataSet>>> CreateNewTrainingProgram([FromBody] CreateTrainingProgramParam createTrainingProgramParam)
        {
            Response<AdminTrainingProgramDataSet> result = await _service.CreateATrainingProgram(createTrainingProgramParam);
            return Ok(result);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPut]
        public async Task<ActionResult<Response<AdminTrainingProgramDataSet>>> UpdateATrainingProgram([FromBody] AdminTrainingProgramDataSet updateTrainingProgramParam)
        {
            Response<AdminTrainingProgramDataSet> result = await _service.UpdateATrainingProgram(updateTrainingProgramParam);
            return Ok(result);
        }
    }
}
