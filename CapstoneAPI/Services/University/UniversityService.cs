using AutoMapper;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.University
{
    public class UniversityService : IUniversityService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public UniversityService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UniversityDataSet>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam)
        {
            WeightNumber weightNumber = await _uow.WeightNumberRepository.GetFirst(filter: w => w.MajorId == universityParam.MajorId && w.SubjectGroupId == universityParam.SubjectGroupId);
            if (weightNumber == null)
            {
                return null;
            }
            IEnumerable<UniversityDataSet> universities = (await _uow.EntryMarkRepository.Get(filter: e => e.WeightNumberId == weightNumber.Id && e.Mark <= universityParam.TotalMark, includeProperties: "University")).OrderByDescending(e => e.Mark).Select(u => _mapper.Map<UniversityDataSet>(u.University));
            return universities;
        }
    }
}
