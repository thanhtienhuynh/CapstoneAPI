namespace CapstoneAPI.Services.TestSubmission
{
    using AutoMapper;
    using CapstoneAPI.DataSets;
    using CapstoneAPI.DataSets.Question;
    using CapstoneAPI.DataSets.TestSubmission;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Models;
    using CapstoneAPI.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
            foreach (QuestionParam submitQuestion in testSubmissionParam.Questions)
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
            TestSubmissionDataSet testSubmissionDataSet = new TestSubmissionDataSet()
            {
                TestId = loadedTest.Id,
                Mark = mark,
                NumberOfRightAnswers = correctAnswer,
                SpentTime = testSubmissionParam.SpentTime,
                SubmissionDate = DateTime.UtcNow,
                NumberOfQuestion = loadedTest.NumberOfQuestion
            };
            return testSubmissionDataSet;
        }

        public async Task<BaseResponse> SaveTestSubmission(SaveTestSubmissionParam saveTestSubmissionParam, string token)
        {
            BaseResponse response = new BaseResponse();
            bool isSuccess = false;
            if (token == null || token.Trim().Length == 0)
            {
                isSuccess = false;
            }
            Models.TestSubmission testSubmission = new Models.TestSubmission()
            {
                TestId = saveTestSubmissionParam.TestId,
                SpentTime = saveTestSubmissionParam.SpentTime,
                SubmissionDate = DateTime.UtcNow,
                NumberOfRightAnswers = saveTestSubmissionParam.NumberOfRightAnswers,
                Mark = saveTestSubmissionParam.Mark,
            };


            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);
            if (userIdString != null && userIdString.Length > 0)
            {
                int userId = Int32.Parse(userIdString);
                testSubmission.UserId = userId;
                _uow.TestSubmissionRepository.Insert(testSubmission);
                if ((await _uow.CommitAsync()) > 0)
                {
                    foreach (QuestionParam questionParam in saveTestSubmissionParam.Questions)
                    {
                        _uow.QuestionSubmisstionRepository.Insert(new QuestionSubmisstion()
                        {
                            QuestionId = questionParam.Id,
                            TestSubmissionId = testSubmission.Id,
                            Result = questionParam.Options
                        });
                    }
                    if ((await _uow.CommitAsync()) > 0)
                    {
                        isSuccess = true;
                    }
                }
            }
            response.isSuccess = isSuccess;
            return response;
        }

        public async Task<IEnumerable<QuestionDataSet>> ScoringTest1()
        {
            IEnumerable<Question> datas = await _uow.QuestionRepository.Get(filter: q => q.TestId == 4);
            return datas.Select(t => _mapper.Map<QuestionDataSet>(t));
        }
    }
}
