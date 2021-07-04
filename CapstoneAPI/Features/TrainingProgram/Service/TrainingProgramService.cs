using AutoMapper;
using CapstoneAPI.Features.TrainingProgram.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.TrainingProgram.Service
{
    public class TrainingProgramService : ITrainingProgramService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<TrainingProgramService>();

        public TrainingProgramService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<Response<IEnumerable<AdminTrainingProgramDataSet>>> AdminGetAllTrainingPrograms()
        {
            Response<IEnumerable<AdminTrainingProgramDataSet>> response = new Response<IEnumerable<AdminTrainingProgramDataSet>>();
            try
            {
                IEnumerable<AdminTrainingProgramDataSet> trainingProgramDataSets = (await _uow.TrainingProgramRepository.Get(filter: t => t.Status == Consts.STATUS_ACTIVE))
             .Select(t => _mapper.Map<AdminTrainingProgramDataSet>(t));
                if (trainingProgramDataSets == null || !trainingProgramDataSets.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không có hệ đào tạo nào trong hệ thống!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = trainingProgramDataSets;
                }
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<AdminTrainingProgramDataSet>> CreateATrainingProgram(CreateTrainingProgramParam createTraining)
        {
            Response<AdminTrainingProgramDataSet> response = new Response<AdminTrainingProgramDataSet>();
            try
            {
                if (createTraining.Name == null || createTraining.Name.Trim().Length <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên hệ đào tạo không được để trống!");
                    return response;
                }
                Models.TrainingProgram existedModel = await _uow.TrainingProgramRepository.GetFirst(filter: t => t.Name == createTraining.Name);
                if (existedModel != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Hệ đào tạo này đã tồn tại!");
                    return response;
                }
                Models.TrainingProgram newTrainingProgram = new Models.TrainingProgram
                {
                    Name = createTraining.Name,
                    Status = Consts.STATUS_ACTIVE,
                };
                _uow.TrainingProgramRepository.Insert(newTrainingProgram);
                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lỗi hệ thống!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = _mapper.Map<AdminTrainingProgramDataSet>(newTrainingProgram);
                }
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<AdminTrainingProgramDataSet>> UpdateATrainingProgram(AdminTrainingProgramDataSet updateTraining)
        {
            Response<AdminTrainingProgramDataSet> response = new Response<AdminTrainingProgramDataSet>();
            try
            {
                if (updateTraining.Name == null || updateTraining.Name.Trim().Length <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên hệ đào tạo không được để trống!");
                    return response;
                }
                Models.TrainingProgram existedModel = await _uow.TrainingProgramRepository.GetFirst(filter: t => t.Name == updateTraining.Name);
                if (existedModel != null && existedModel.Id != updateTraining.Id)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Hệ đào tạo này đã tồn tại!");
                    return response;
                }
                Models.TrainingProgram newUpdateTrainingProgram = await _uow.TrainingProgramRepository.GetFirst(filter: t => t.Id == updateTraining.Id);
                if (newUpdateTrainingProgram == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Hệ đào tạo không tồn tại trong hệ thống!");
                    return response;
                }
                newUpdateTrainingProgram.Name = updateTraining.Name;
                newUpdateTrainingProgram.Status = updateTraining.Status;
                _uow.TrainingProgramRepository.Update(newUpdateTrainingProgram);
                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lỗi hệ thống!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = _mapper.Map<AdminTrainingProgramDataSet>(newUpdateTrainingProgram);
                }
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }
    }
}
