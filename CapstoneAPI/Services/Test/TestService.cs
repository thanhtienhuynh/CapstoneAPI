namespace CapstoneAPI.Services.Test
{
    using AutoMapper;
    using CapstoneAPI.DataSets.Question;
    using CapstoneAPI.DataSets.Test;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Models;
    using CapstoneAPI.Repositories;
    using CapstoneAPI.Wrappers;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TestService : ITestService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<TestService>();

        public TestService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<List<SubjectBasedTestDataSet>>> GetFilteredTests(TestParam testParam)
        {
            Response<List<SubjectBasedTestDataSet>> response = new Response<List<SubjectBasedTestDataSet>>();
            try
            {
                List<SubjectBasedTestDataSet> testsReponse = new List<SubjectBasedTestDataSet>();
                List<int> subjectIds = new List<int>();
                IEnumerable<SubjectGroupDetail> subjectGroupDetails = null;
                if (testParam.SubjectGroupId > 0)
                {
                    subjectGroupDetails = await _uow.SubjecGroupDetailRepository
                                            .Get(filter: s => s.SubjectGroupId == testParam.SubjectGroupId,
                                                includeProperties: "SpecialSubjectGroup,SpecialSubjectGroup.Subjects");
                }

                if (!subjectGroupDetails.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Hệ thống ghi nhận không có môn học trong khối đã chọn!");
                    return response;
                }

                foreach (SubjectGroupDetail subjectGroupDetail in subjectGroupDetails)
                {
                    if (subjectGroupDetail.SubjectId != null)
                    {
                        subjectIds.Add((int)subjectGroupDetail.SubjectId);
                    }
                    else if (subjectGroupDetail.SpecialSubjectGroupId != null && subjectGroupDetail.SpecialSubjectGroup.Subjects != null && subjectGroupDetail.SpecialSubjectGroup.Subjects.Any())
                    {
                        subjectIds.AddRange(subjectGroupDetail.SpecialSubjectGroup.Subjects.Select(s => s.Id));
                    }
                }
                if (subjectIds != null && subjectIds.Any())
                {
                    foreach (int subjectId in subjectIds)
                    {
                        IEnumerable<Test> clasifiedTests = await _uow.TestRepository
                                                    .Get(filter: test => test.Status == Consts.STATUS_ACTIVE
                                                        && test.TestTypeId == Consts.TEST_HT_TYPE_ID
                                                        && test.SubjectId == subjectId
                                                        && test.UniversityId == null);
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
                if (testsReponse.Any())
                {
                    response.Succeeded = true;
                    response.Data = testsReponse;
                }
                else
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không có bài thi phù hợp");
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

        public async Task<Response<TestDataSet>> GetTestById(int id)
        {
            Response<TestDataSet> response = new Response<TestDataSet>();
            try
            {
                Test test = await _uow.TestRepository.GetFirst(filter: t => t.Id == id && t.Status == Consts.STATUS_ACTIVE,
                                                                    includeProperties: "Questions,Questions.Options");
                if (test == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bài thi không tồn tại!");
                    return response;
                }
                test.Questions = test.Questions.OrderBy(s => s.Ordinal).ToList();
                TestDataSet testDataSet = _mapper.Map<TestDataSet>(test);
                int realOrder = 0;
                foreach (QuestionDataSet questionDataSet in testDataSet.Questions)
                {
                    if (!questionDataSet.IsAnnotate)
                    {
                        questionDataSet.RealOrder = realOrder++;
                    }
                    questionDataSet.Options = questionDataSet.Options.OrderBy(o => o.Ordinal).ToList();
                }
                response.Succeeded = true;
                response.Data = testDataSet;
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
