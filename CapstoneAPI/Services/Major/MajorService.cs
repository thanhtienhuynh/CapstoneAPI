using AutoMapper;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Major
{
    public class MajorService : IMajorService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<MajorService>();

        public MajorService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<Response<IEnumerable<AdminMajorDataSet>>> GetActiveMajorsByAdmin()
        {
            Response<IEnumerable<AdminMajorDataSet>> response = new Response<IEnumerable<AdminMajorDataSet>>();
            try
            {
                IEnumerable<AdminMajorDataSet> adminMajorDataSets = (await _uow.MajorRepository.Get(filter: m => m.Status == Consts.STATUS_ACTIVE))
                .Select(m => _mapper.Map<AdminMajorDataSet>(m));
                if (adminMajorDataSets == null || !adminMajorDataSets.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không có ngành học thỏa mãn!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = adminMajorDataSets;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<ResultOfCreateMajorDataSet>> CreateAMajor(CreateMajorDataSet createMajorDataSet)
        {
            Response<ResultOfCreateMajorDataSet> response = new Response<ResultOfCreateMajorDataSet>();
            try
            {
                if (createMajorDataSet.Name.Equals("") || createMajorDataSet.Code.Trim().Equals(""))
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành không được để trống!");
                    return response;
                }
                Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(createMajorDataSet.Code) && m.Status == Consts.STATUS_ACTIVE);
                if (existMajor != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành đã tồn tại!");
                    return response;
                }
                existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(createMajorDataSet.Name) && m.Status == Consts.STATUS_ACTIVE);
                if (existMajor != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên ngành đã tồn tại!");
                    return response;
                }
                Models.Major newMajor = _mapper.Map<Models.Major>(createMajorDataSet);
                newMajor.Status = Consts.STATUS_ACTIVE;
                _uow.MajorRepository.Insert(newMajor);
                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên ngành đã tồn tại!");
                }
                else
                {
                    response.Succeeded = false;
                    response.Data = _mapper.Map<ResultOfCreateMajorDataSet>(newMajor);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<ResultOfCreateMajorDataSet>> UpdateAMajor(ResultOfCreateMajorDataSet updateMajor)
        {
            Response<ResultOfCreateMajorDataSet> response = new Response<ResultOfCreateMajorDataSet>();
            try
            {
                if (updateMajor.Name.Trim().Equals("") || updateMajor.Code.Trim().Equals("") || (updateMajor.Status != Consts.STATUS_ACTIVE && updateMajor.Status != Consts.STATUS_INACTIVE))
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Dữ liệu bị thiếu!");
                    return response;
                }
                Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(updateMajor.Code) && m.Status == Consts.STATUS_ACTIVE);
                if (existMajor != null && existMajor.Id != updateMajor.Id)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành cập nhật đã tồn tại!");
                    return response;
                }
                existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(updateMajor.Name) && m.Status == Consts.STATUS_ACTIVE);
                if (existMajor != null && existMajor.Id != updateMajor.Id)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên ngành cập nhật đã tồn tại!");
                    return response;
                }
                Models.Major objToUpdate = await _uow.MajorRepository.GetFirst(filter: m => m.Id.Equals(updateMajor.Id));
                if (objToUpdate == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Ngành này không tồn tại trong hệ thống!");
                    return response;
                }
                objToUpdate.Code = updateMajor.Code;
                objToUpdate.Name = updateMajor.Name;
                objToUpdate.Status = updateMajor.Status;
                _uow.MajorRepository.Update(objToUpdate);
                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Ngành này không tồn tại trong hệ thống!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = _mapper.Map<ResultOfCreateMajorDataSet>(objToUpdate);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
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
