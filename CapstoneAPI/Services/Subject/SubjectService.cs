using AutoMapper;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Major;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Subject
{
    public class SubjectService : ISubjectService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public SubjectService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SubjectDataSet>> GetAllSubjects()
        {
            IEnumerable<SubjectDataSet> subjects = (await _uow.SubjectRepository.Get(filter: s => s.Status == Consts.STATUS_ACTIVE)).Select(s => _mapper.Map<SubjectDataSet>(s));
            return subjects;
        }
    }
}
