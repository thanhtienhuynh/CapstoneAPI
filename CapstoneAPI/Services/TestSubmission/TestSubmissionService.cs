using AutoMapper;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.DataSets.TestSubmission;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.TestSubmission
{
    public class TestSubmissionService : ITestSubmissionService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public TestSubmissionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<TestSubmissionDataSet> ScoringTest(TestSubmissionParam testSubmissionParam)
        {
            int correctAnswer = 0;
            Models.Test loadedTest = await _uow.TestRepository.GetById(testSubmissionParam.TestId);
            if (loadedTest == null)
            {
                return null;
            }
            foreach(QuestionParam submitQuestion in testSubmissionParam.Questions)
            {
                Question loadedQuestion = await _uow.QuestionRepository.GetFirst(
                    filter: q => q.Id == submitQuestion.Id 
                            && q.TestId == testSubmissionParam.TestId 
                            && q.Result.Trim() == submitQuestion.Options.Trim());
                if (loadedQuestion != null)
                {
                    correctAnswer++;
                }
            }
            double mark = Consts.DEFAULT_MAX_SCORE * ((double)correctAnswer / (double)loadedTest.NumberOfQuestion);
            TestSubmissionDataSet testSubmissionDataSet = new TestSubmissionDataSet() {
                TestId = loadedTest.Id,
                Mark = mark,
                NumberOfRightAnswers = correctAnswer,
                SpentTime = testSubmissionParam.SpentTime,
                SubmissionDate = DateTime.UtcNow,
                NumberOfQuestion = loadedTest.NumberOfQuestion
            };
            return testSubmissionDataSet;
        }

        public async Task<IEnumerable<QuestionDataSet>> ScoringTest1()
        {
            IEnumerable<Question> datas = await _uow.QuestionRepository.Get(filter: q => q.TestId == 4);
            return datas.Select(t => _mapper.Map<QuestionDataSet>(t));
        }

    }
}
