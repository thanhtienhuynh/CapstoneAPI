using AutoMapper;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
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
        public MajorService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<IEnumerable<AdminMajorDataSet>> GetActiveMajorsByAdmin()
        {
            return (await _uow.MajorRepository.Get(filter: m => m.Status == Consts.STATUS_ACTIVE))
                .Select(m => _mapper.Map<AdminMajorDataSet>(m));
        }

        public async Task<ResultOfCreateMajorDataSet> CreateAMajor(CreateMajorDataSet createMajorDataSet)
        {
            if(createMajorDataSet.Name.Equals("")|| createMajorDataSet.Code.Equals(""))
            {
                return null;
            }
            Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(createMajorDataSet.Code));
            if(existMajor != null)
            {
                return null;
            }
            existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(createMajorDataSet.Name));
            if (existMajor != null)
            {
                return null;
            }
            Models.Major newMajor = _mapper.Map<Models.Major>(createMajorDataSet);
            _uow.MajorRepository.Insert(newMajor);
            int result = await _uow.CommitAsync();
            if(result > 0)
            {
                return _mapper.Map<ResultOfCreateMajorDataSet>(newMajor);
            }
            return null;
        }

        public async Task<ResultOfCreateMajorDataSet> UpdateAMajor(ResultOfCreateMajorDataSet updateMajor)
        {
            if (updateMajor.Name.Equals("") || updateMajor.Code.Equals("") || (updateMajor.Status != Consts.STATUS_ACTIVE&& updateMajor.Status != Consts.STATUS_INACTIVE))
            {
                return null;
            }
            Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(updateMajor.Code));
            if (existMajor != null && existMajor.Id != updateMajor.Id)
            {
                return null;
            }
            existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(updateMajor.Name));
            if (existMajor != null && existMajor.Id != updateMajor.Id)
            {
                return null;
            }
            Models.Major objToUpdate = await _uow.MajorRepository.GetFirst(filter: m => m.Id.Equals(updateMajor.Id));
            if (objToUpdate == null)
            {
                return null;
            }
            objToUpdate.Code = updateMajor.Code;
            objToUpdate.Name = updateMajor.Name;
            objToUpdate.Status = updateMajor.Status;
            _uow.MajorRepository.Update(objToUpdate);
            int result = await _uow.CommitAsync();
            if(result > 0)
            {
                return _mapper.Map<ResultOfCreateMajorDataSet>(objToUpdate);
            }
            return null;
        }
    }
}
