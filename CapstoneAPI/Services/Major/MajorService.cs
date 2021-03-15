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
    }
}
