using CapstoneAPI.DataSets.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Models;
using AutoMapper;
using CapstoneAPI.Repositories;
using CapstoneAPI.Helpers;
using CapstoneAPI.DataSets.Question;

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
        public async Task<List<SubjectBasedTestDataSet>> GetFilteredTests(TestParam testParam)
        {
            List<SubjectBasedTestDataSet> testsReponse = new List<SubjectBasedTestDataSet>();
            IEnumerable<int> subjectIds = null;
            if (testParam.SubjectGroupId > 0)
            {
                subjectIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == testParam.SubjectGroupId))
                                                                                .Select(s => s.SubjectId);
            }
            IEnumerable<Models.Test> tests = await _uow.TestRepository.Get(filter: test => test.Status == Consts.STATUS_ACTIVE);
            if (subjectIds != null && subjectIds.Any())
            { 
                foreach (int subjectId in subjectIds)
                {
                    IEnumerable<Models.Test> clasifiedTests = null;
                    clasifiedTests = tests.Where(t => t.SubjectId == subjectId && t.UniversityId == null);
                    if (clasifiedTests.Any())
                    {
                        testsReponse.Add(new SubjectBasedTestDataSet()
                        {
                            SubjectId = subjectId,
                            Tests = clasifiedTests.Select(t => _mapper.Map<TestDataSet>(t)).ToList(),
                            UniversityId = null
                        }
                        );
                    }
                }
            }

            if (testParam.UniversityId > 0)
            {
                tests = tests.Where(t => t.UniversityId == testParam.UniversityId);
                if (tests.Any())
                {
                    testsReponse.Add(new SubjectBasedTestDataSet()
                        {
                        SubjectId = null,
                        Tests = tests.Select(t => _mapper.Map<TestDataSet>(t)).ToList(),
                        UniversityId = testParam.UniversityId
                        }
                    );
                }
                
            }
            

            return testsReponse;                               
        }

        public async Task<TestDataSet> GetTestById(int id)
        {
            Models.Test test = await _uow.TestRepository.GetFirst(filter: t => t.Id == id && t.Status == Consts.STATUS_ACTIVE,
                                                                    includeProperties: "Questions,Questions.Options");
            test.Questions = test.Questions.OrderBy(s => s.Ordinal).ToList();
            foreach(Question questionDataSet in test.Questions)
            {
                questionDataSet.Options = questionDataSet.Options.OrderBy(o => o.Ordinal).ToList();
            }
            return _mapper.Map<TestDataSet>(test);

        }
    }
}
