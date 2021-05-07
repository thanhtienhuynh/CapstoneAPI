using CapstoneAPI.DataSets.TrainingProgram;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.TrainingProgram
{
   public interface ITrainingProgramService
    {
        Task<Response<IEnumerable<AdminTrainingProgramDataSet>>> AdminGetAllTrainingPrograms();

        Task<Response<AdminTrainingProgramDataSet>> CreateATrainingProgram(CreateTrainingProgramParam createTraining);
        Task<Response<AdminTrainingProgramDataSet>> UpdateATrainingProgram(AdminTrainingProgramDataSet updateTraining);
    }
}
