using AutoMapper;
using CapstoneAPI.DataSets.TrainingProgram;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.TrainingProgram
{
    public class TrainingProgramService : ITrainingProgramService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public TrainingProgramService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<IEnumerable<AdminTrainingProgramDataSet>> AdminGetAllTrainingPrograms()
        {
            return (await _uow.TrainingProgramRepository.Get(filter: t => t.Status == Consts.STATUS_ACTIVE))
              .Select(t => _mapper.Map<AdminTrainingProgramDataSet>(t));
        }

        public async Task<AdminTrainingProgramDataSet> CreateATrainingProgram(CreateTrainingProgramParam createTraining)
        {
            if(createTraining.Name == null || createTraining.Name.Length <= 0)
            {
                return null;
            }
            Models.TrainingProgram existedModel = await  _uow.TrainingProgramRepository.GetFirst(filter: t => t.Name == createTraining.Name);
            if(existedModel != null)
            {
                return null;
            }
            Models.TrainingProgram newTrainingProgram = new Models.TrainingProgram
            {
                Name = createTraining.Name,
                Status = Consts.STATUS_ACTIVE,
            };
            _uow.TrainingProgramRepository.Insert(newTrainingProgram);
            int result = await _uow.CommitAsync();
            if(result > 0)
            {
                return _mapper.Map<AdminTrainingProgramDataSet>(newTrainingProgram);
            }
            return null;
        }

        public async Task<AdminTrainingProgramDataSet> UpdateATrainingProgram(AdminTrainingProgramDataSet updateTraining)
        {

            if (updateTraining.Name == null || updateTraining.Name.Length <= 0 || updateTraining.Id <= 0)
            {
                return null;
            }
            Models.TrainingProgram existedModel = await _uow.TrainingProgramRepository.GetFirst(filter: t => t.Name == updateTraining.Name);
            if (existedModel != null && existedModel.Id != updateTraining.Id)
            {
                return null;
            }
            Models.TrainingProgram newUpdateTrainingProgram = await _uow.TrainingProgramRepository.GetFirst(filter: t => t.Id == updateTraining.Id);
            if(newUpdateTrainingProgram == null)
            {
                return null;
            }
            newUpdateTrainingProgram.Name = updateTraining.Name;
            newUpdateTrainingProgram.Status = updateTraining.Status;
            _uow.TrainingProgramRepository.Update(newUpdateTrainingProgram);
            int result = await _uow.CommitAsync();
            if (result > 0)
            {
                return _mapper.Map<AdminTrainingProgramDataSet>(newUpdateTrainingProgram);
            }
            return null;
        }
    }
}
