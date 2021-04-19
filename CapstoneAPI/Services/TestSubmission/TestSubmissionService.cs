namespace CapstoneAPI.Services.TestSubmission
{
    using AutoMapper;
    using CapstoneAPI.DataSets;
    using CapstoneAPI.DataSets.Option;
    using CapstoneAPI.DataSets.Question;
    using CapstoneAPI.DataSets.QuestionSubmission;
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

        public async Task<BaseResponse<TestSubmission>> SaveTestSubmission(SaveTestSubmissionParam saveTestSubmissionParam, string token)
        {
            BaseResponse<TestSubmission> response = new BaseResponse<TestSubmission>();
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
                Mark = Math.Round(saveTestSubmissionParam.Mark, 2),
            };


            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);
            if (userIdString != null && userIdString.Length > 0)
            {
                int userId = Int32.Parse(userIdString);
                bool isMajorCared = (await _uow.UserMajorRepository
                    .GetFirst(filter: u => u.MajorId == saveTestSubmissionParam.MajorId && u.UserId == userId)) != null;
                if (!isMajorCared)
                {
                    _uow.UserMajorRepository.Insert(new UserMajor()
                    {
                        MajorId = saveTestSubmissionParam.MajorId,
                        UserId = userId
                    });
                }

                bool isUniversityCared = (await _uow.UserUniversityRepository
                    .GetFirst(filter: u => u.UniversityId == saveTestSubmissionParam.UniversityId && u.UserId == userId)) != null;

                if (!isUniversityCared)
                {
                    _uow.UserUniversityRepository.Insert(new UserUniversity()
                    {
                        UniversityId = saveTestSubmissionParam.UniversityId,
                        UserId = userId
                    });
                }

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
            response.IsSuccess = isSuccess;
            return response;
        }

        public async Task<List<UserTestSubmissionDataSet>> GetTestSubmissionsByUser(string token)
        {
            if (token == null || token.Trim().Length == 0)
            {
                return null;
            }
            
            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);
            if (userIdString != null && userIdString.Length > 0)
            {
                int userId = Int32.Parse(userIdString);
                IEnumerable<TestSubmission> testSubmissionDataSets = (await _uow.TestSubmissionRepository
                    .Get(filter: t => t.UserId == userId,
                        includeProperties: "Test",
                        orderBy: t => t.OrderBy(t => t.SpentTime))).GroupBy(t => t.TestId).Select(g => g.Last());
                if (!testSubmissionDataSets.Any())
                {
                    return null;
                }
                List<UserTestSubmissionDataSet> userTestSubmissionDataSets = new List<UserTestSubmissionDataSet>();
                foreach(TestSubmission testSubmission in testSubmissionDataSets)
                {
                    UserTestSubmissionDataSet userTestSubmissionDataSet = _mapper.Map<UserTestSubmissionDataSet>(testSubmission);
                    userTestSubmissionDataSet.TimeLimit = (int) testSubmission.Test.TimeLimit;
                    userTestSubmissionDataSet.NumberOfQuestion = testSubmission.Test.NumberOfQuestion;
                    userTestSubmissionDataSet.NumberOfCompletion = (await _uow.TestSubmissionRepository
                        .Get(filter: t => t.UserId == userId && t.TestId == testSubmission.TestId)).Count();
                    userTestSubmissionDataSet.TestName = testSubmission.Test.Name;
                    userTestSubmissionDataSets.Add(userTestSubmissionDataSet);
                }
                return userTestSubmissionDataSets;
            }

            return null;
        }

        public async Task<DetailTestSubmissionDataSet> GetDetailTestSubmissionByUser(int testSubmissionId, string token)
        {
            if (token == null || token.Trim().Length == 0)
            {
                return null;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Trim().Length <= 0)
            {
                return null;
            }
            int userId = Int32.Parse(userIdString);
            TestSubmission testSubmission = await _uow.TestSubmissionRepository
               .GetFirst(filter: t => t.Id == testSubmissionId && t.UserId == userId,
                        includeProperties: "Test");

            if (testSubmission == null)
            {
                return null;
            }

            List<QuestionSubmissionDataSet> questionSubmissionDataSets = new List<QuestionSubmissionDataSet>();


            IEnumerable<QuestionSubmisstion> questionSubmissions = (await _uow.QuestionSubmisstionRepository
                .Get(filter: q => q.TestSubmissionId == testSubmission.Id,
                 includeProperties: "Question,Question.Options")).OrderBy(q => q.Question.Ordinal);
            foreach (QuestionSubmisstion questionSubmission in questionSubmissions)
            {
                QuestionSubmissionDataSet questionSubmissionDataSet = _mapper.Map<QuestionSubmissionDataSet>(questionSubmission);
                questionSubmissionDataSet.RightResult = questionSubmission.Question.Result;
                questionSubmissionDataSet.QuestionContent = questionSubmission.Question.QuestionContent;
                questionSubmissionDataSet.NumberOfOption = questionSubmission.Question.NumberOfOption;
                questionSubmissionDataSet.Type = questionSubmission.Question.Type;
                questionSubmissionDataSet.TestId = questionSubmission.Question.TestId;
                questionSubmissionDataSet.Options = questionSubmission.Question.Options.OrderBy(o => o.Ordinal).Select(o => _mapper.Map<OptionDataSet>(o)).ToList();
                questionSubmissionDataSets.Add(questionSubmissionDataSet);
            }

            DetailTestSubmissionDataSet detailTestSubmissionDataSet = _mapper.Map<DetailTestSubmissionDataSet>(testSubmission);

            detailTestSubmissionDataSet.NumberOfCompletion = (await _uow.TestSubmissionRepository
                        .Get(filter: t => t.UserId == userId && t.TestId == testSubmission.TestId)).Count();
            detailTestSubmissionDataSet.QuestionSubmissions = questionSubmissionDataSets;
            detailTestSubmissionDataSet.NumberOfQuestion = testSubmission.Test.NumberOfQuestion;
            detailTestSubmissionDataSet.TimeLimit = (int) testSubmission.Test.TimeLimit;
            detailTestSubmissionDataSet.TestName = testSubmission.Test.Name;

            return detailTestSubmissionDataSet;
        }

        public async Task<IEnumerable<QuestionDataSet>> ScoringTest1()
        {
            IEnumerable<Question> datas = await _uow.QuestionRepository.Get(filter: q => q.TestId == 4);
            return datas.Select(t => _mapper.Map<QuestionDataSet>(t));
        }
    }
}
