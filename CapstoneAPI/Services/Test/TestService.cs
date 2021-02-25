using CapstoneAPI.DataSets.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Models;
using AutoMapper;
using CapstoneAPI.Repositories;
using CapstoneAPI.Helpers;

namespace CapstoneAPI.Services.Test
{
    public class TestService : ITestService
    {

        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public TestService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<IEnumerable<TestDataSet>> GetFilteredTests(TestParam testParam)
        {
            IEnumerable<int> subjectIds = null;
            if (testParam.SubjectGroupId > 0)
            {
                subjectIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == testParam.SubjectGroupId))
                                                                                .Select(s => s.SubjectId);
            }
            IEnumerable<Models.Test> tests = await _uow.TestRepository.Get(filter: test => test.Status == Consts.STATUS_ACTIVE);
            if (subjectIds != null && subjectIds.Any())
            {
                tests = tests.Where(t => subjectIds.Contains((int) t.SubjectId));
            }

            if (testParam.UniversityId > 0)
            {
                tests = tests.Where(t => t.UniversityId == testParam.UniversityId || t.UniversityId == null);
            } else
            {
                tests = tests.Where(t => t.UniversityId == null);
            }

            return tests.Select(t => _mapper.Map<TestDataSet>(t));                               
        }

        public async Task<TestDataSet> GetTestById(int id)
        {
            Models.Test test = await _uow.TestRepository.GetFirst(filter: t => t.Id == id && t.Status == Consts.STATUS_ACTIVE,
                                                                    includeProperties: "Questions,Questions.Options");
            return _mapper.Map<TestDataSet>(test);

        }
    }
}
