using CapstoneAPI.DataSets.TrainingProgram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.TrainingProgram
{
   public interface ITrainingProgramService
    {
        Task<IEnumerable<AdminTrainingProgramDataSet>> AdminGetAllTrainingPrograms();

        Task<AdminTrainingProgramDataSet> CreateATrainingProgram(CreateTrainingProgramParam createTraining);
        Task<AdminTrainingProgramDataSet> UpdateATrainingProgram(AdminTrainingProgramDataSet updateTraining);
    }
}
