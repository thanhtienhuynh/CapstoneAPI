using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.TrainingProgram;
using CapstoneAPI.Services.TrainingProgram;
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
        public async Task<ActionResult<IEnumerable<AdminTrainingProgramDataSet>>> GetTrainingPrograms()
        {
            IEnumerable<AdminTrainingProgramDataSet> trainingPrograms = await _service.AdminGetAllTrainingPrograms();
            if (!trainingPrograms.Any())
            {
                return NotFound();
            }
            return Ok(trainingPrograms);
        }

        [HttpPost]
        public async Task<ActionResult<AdminTrainingProgramDataSet>> CreateNewTrainingProgram([FromBody] CreateTrainingProgramParam createTrainingProgramParam)
        {
            AdminTrainingProgramDataSet result = await _service.CreateATrainingProgram(createTrainingProgramParam);
            if(result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpPut]
        public async Task<ActionResult<AdminTrainingProgramDataSet>> UpdateATrainingProgram([FromBody] AdminTrainingProgramDataSet updateTrainingProgramParam)
        {

            AdminTrainingProgramDataSet result = await _service.UpdateATrainingProgram(updateTrainingProgramParam);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
