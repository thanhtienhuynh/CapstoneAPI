namespace CapstoneAPI.Features.TestSubmission.Service
{
    using AutoMapper;
    using CapstoneAPI.DataSets;
    using CapstoneAPI.DataSets.Option;
    using CapstoneAPI.DataSets.Question;
    using CapstoneAPI.DataSets.QuestionSubmission;
    using CapstoneAPI.Features.TestSubmission.DataSet;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Models;
    using CapstoneAPI.Repositories;
    using CapstoneAPI.Wrappers;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class TestSubmissionService : ITestSubmissionService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<TestSubmissionService>();

        public TestSubmissionService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<TestSubmissionDataSet>> ScoringTest(TestSubmissionParam testSubmissionParam)
        {
            Response<TestSubmissionDataSet> response = new Response<TestSubmissionDataSet>();
            List<ResultQuestion> resultQuestions = new List<ResultQuestion>();
            try
            {
                int correctAnswer = 0;
                Models.Test loadedTest = await _uow.TestRepository.GetById(testSubmissionParam.TestId);
                if (loadedTest == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bài thi không tồn tại!");
                    return response;
                }
                foreach (QuestionParam submitQuestion in testSubmissionParam.Questions)
                {
                    Question loadedQuestion = await _uow.QuestionRepository.GetFirst(
                        filter: q => q.Id == submitQuestion.Id
                                && q.TestId == testSubmissionParam.TestId);
                    if (loadedQuestion == null)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Câu hỏi không tồn tại!");
                        return response;
                    }
                    if (loadedQuestion.Result.Trim() == submitQuestion.Options.Trim())
                    {
                        correctAnswer++;
                    }
                    resultQuestions.Add(new ResultQuestion() { Id = loadedQuestion.Id, Result = loadedQuestion.Result });
                }
                double mark = Consts.DEFAULT_MAX_SCORE * ((double)correctAnswer / (double)loadedTest.NumberOfQuestion);
                TestSubmissionDataSet testSubmissionDataSet = new TestSubmissionDataSet()
                {
                    TestId = loadedTest.Id,
                    Mark = mark,
                    NumberOfRightAnswers = correctAnswer,
                    SpentTime = testSubmissionParam.SpentTime,
                    SubmissionDate = DateTime.UtcNow,
                    NumberOfQuestion = loadedTest.NumberOfQuestion,
                    SubjectId = (int)loadedTest.SubjectId,
                    ResultQuestions = resultQuestions
                };
                response.Succeeded = true;
                response.Data = testSubmissionDataSet;
            }
            catch (Exception ex)
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

        public async Task<Response<bool>> SaveTestSubmissions(List<SaveTestSubmissionParam> saveTestSubmissionParams, string token)
        {
            Response<bool> response = new Response<bool>();

            if (token == null || token.Trim().Length == 0)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Trim().Length <= 0)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Tài khoản của bạn không tồn tại!");
            }

            int userId = Int32.Parse(userIdString);

            using var tran = _uow.GetTransaction();
            try
            {
                foreach (SaveTestSubmissionParam saveTestSubmissionParam in saveTestSubmissionParams)
                {
                    TestSubmission testSubmission = new TestSubmission()
                    {
                        TestId = saveTestSubmissionParam.TestId,
                        SpentTime = saveTestSubmissionParam.SpentTime,
                        SubmissionDate = DateTime.UtcNow,
                        NumberOfRightAnswers = saveTestSubmissionParam.NumberOfRightAnswers,
                        Mark = Math.Round(saveTestSubmissionParam.Mark, 2),
                        UserId = userId
                    };

                    _uow.TestSubmissionRepository.Insert(testSubmission);
                    int subjectId = (int)(await _uow.TestRepository.GetById(saveTestSubmissionParam.TestId)).SubjectId;

                    Transcript transcript = await _uow.TranscriptRepository.GetFirst(t => t.TranscriptTypeId == 3 && t.UserId == userId && t.SubjectId == subjectId);
                    if (transcript != null)
                    {
                        transcript.Mark = Math.Round(saveTestSubmissionParam.Mark, 2);
                        transcript.DateRecord = DateTime.UtcNow;
                        transcript.IsUpdate = true;
                        _uow.TranscriptRepository.Update(transcript);
                    }
                    else
                    {
                        _uow.TranscriptRepository.Insert(new Transcript()
                        {
                            UserId = userId,
                            DateRecord = DateTime.UtcNow,
                            Mark = Math.Round(saveTestSubmissionParam.Mark, 2),
                            TranscriptTypeId = 3,
                            SubjectId = subjectId,
                            IsUpdate = true
                        });
                    }

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
                            response.Succeeded = true;
                            response.Data = true;
                        }
                        else
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Lưu không thành công, lỗi hệ thống!");
                        }
                    }
                    else
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Lưu không thành công, lỗi hệ thống!");
                    }
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                tran.Rollback();
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add(ex.Message);
                return response;
            }
            return response;
        }

        public async Task<Response<List<UserTestSubmissionDataSet>>> GetTestSubmissionsByUser(string token, UserTestSubmissionQueryParam param)
        {
            Response<List<UserTestSubmissionDataSet>> response = new Response<List<UserTestSubmissionDataSet>>();
            try
            {
                if (token == null || token.Trim().Length == 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }



                string userIdString = JWTUtils.GetUserIdFromJwtToken(token);
                if (string.IsNullOrEmpty(userIdString) || Int32.TryParse(userIdString, out int userId))
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn không tồn tại!");
                }

                userId = Int32.Parse(userIdString);
                IEnumerable<TestSubmission> testSubmissionDataSets = (await _uow.TestSubmissionRepository
                    .Get(filter: t => t.UserId == userId,
                        includeProperties: "Test",
                        orderBy: t => t.OrderBy(t => t.SpentTime))).GroupBy(t => t.TestId).Select(g => g.Last());
                if (param.SubjectId != null)
                {
                    testSubmissionDataSets = testSubmissionDataSets.Where(t => t.Test.SubjectId == param.SubjectId);
                }
                if (param.TestTypeId != null)
                {
                    testSubmissionDataSets = testSubmissionDataSets.Where(t => t.Test.TestTypeId == param.TestTypeId);
                }
                if (param.IsSuggestedTest != null)
                {
                    testSubmissionDataSets = testSubmissionDataSets.Where(t => t.Test.IsSuggestedTest == param.IsSuggestedTest);
                }
                switch (param.Order ?? 2)
                {
                    case 1:
                        testSubmissionDataSets.OrderByDescending(a => a.SubmissionDate);
                        break;
                    case 2:
                        testSubmissionDataSets.OrderBy(a => a.SubmissionDate);
                        break;
                    case 3:
                        testSubmissionDataSets.OrderByDescending(a => a.Test.Year);
                        break;
                    case 4:
                        testSubmissionDataSets.OrderBy(a => a.Test.Year);
                        break;
                    case 5:
                        testSubmissionDataSets.OrderByDescending(a => a.Test.Name);
                        break;
                    case 6:
                        testSubmissionDataSets.OrderBy(a => a.Test.Name);
                        break;
                }
                if (!testSubmissionDataSets.Any())
                {
                    response.Succeeded = true;
                    return response;
                }
                List<UserTestSubmissionDataSet> userTestSubmissionDataSets = new List<UserTestSubmissionDataSet>();
                foreach (TestSubmission testSubmission in testSubmissionDataSets)
                {
                    UserTestSubmissionDataSet userTestSubmissionDataSet = _mapper.Map<UserTestSubmissionDataSet>(testSubmission);
                    userTestSubmissionDataSet.TimeLimit = (int)testSubmission.Test.TimeLimit;
                    userTestSubmissionDataSet.NumberOfQuestion = testSubmission.Test.NumberOfQuestion;
                    userTestSubmissionDataSet.NumberOfCompletion = (await _uow.TestSubmissionRepository
                        .Get(filter: t => t.UserId == userId && t.TestId == testSubmission.TestId)).Count();
                    userTestSubmissionDataSet.TestName = testSubmission.Test.Name;
                    userTestSubmissionDataSet.SubmissionDate = DateTime.Parse(userTestSubmissionDataSet.SubmissionDate.ToString("MM/dd/yyyy h:mm tt"));
                    userTestSubmissionDataSets.Add(userTestSubmissionDataSet);
                }


                response.Succeeded = true;
                response.Data = userTestSubmissionDataSets;
               
            }
            catch (Exception ex)
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

        public async Task<Response<DetailTestSubmissionDataSet>> GetDetailTestSubmissionByUser(int testSubmissionId, string token)
        {
            Response<DetailTestSubmissionDataSet> response = new Response<DetailTestSubmissionDataSet>();

            try
            {
                if (token == null || token.Trim().Length == 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

                if (userIdString == null || userIdString.Trim().Length <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn không tồn tại!");
                    return response;
                }
                int userId = Int32.Parse(userIdString);
                TestSubmission testSubmission = await _uow.TestSubmissionRepository
                   .GetFirst(filter: t => t.Id == testSubmissionId && t.UserId == userId,
                            includeProperties: "Test");

                if (testSubmission == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bài thi không tồn tại trong hệ thống!");
                    return response;
                }

                List<QuestionSubmissionDataSet> questionSubmissionDataSets = new List<QuestionSubmissionDataSet>();


                IEnumerable<QuestionSubmisstion> questionSubmissions = (await _uow.QuestionSubmisstionRepository
                    .Get(filter: q => q.TestSubmissionId == testSubmission.Id,
                     includeProperties: "Question,Question.Options")).OrderBy(q => q.Question.Ordinal);
                int realOrder = 0;
                foreach (QuestionSubmisstion questionSubmission in questionSubmissions)
                {
                    QuestionSubmissionDataSet questionSubmissionDataSet = _mapper.Map<QuestionSubmissionDataSet>(questionSubmission);
                    questionSubmissionDataSet.RightResult = questionSubmission.Question.Result;
                    questionSubmissionDataSet.Content = questionSubmission.Question.Content;
                    questionSubmissionDataSet.NumberOfOption = questionSubmission.Question.NumberOfOption;
                    questionSubmissionDataSet.Type = questionSubmission.Question.Type;
                    questionSubmissionDataSet.TestId = questionSubmission.Question.TestId;
                    questionSubmissionDataSet.Options = questionSubmission.Question.Options.OrderBy(o => o.Ordinal).Select(o => _mapper.Map<OptionDataSet>(o)).ToList();
                    questionSubmissionDataSet.IsAnnotate = questionSubmission.Question.IsAnnotate;
                    if (!questionSubmissionDataSet.IsAnnotate)
                    {
                        questionSubmissionDataSet.RealOrder = realOrder++;
                    }
                    questionSubmissionDataSets.Add(questionSubmissionDataSet);
                }

                DetailTestSubmissionDataSet detailTestSubmissionDataSet = _mapper.Map<DetailTestSubmissionDataSet>(testSubmission);

                detailTestSubmissionDataSet.NumberOfCompletion = (await _uow.TestSubmissionRepository
                            .Get(filter: t => t.UserId == userId && t.TestId == testSubmission.TestId)).Count();
                detailTestSubmissionDataSet.QuestionSubmissions = questionSubmissionDataSets;
                detailTestSubmissionDataSet.NumberOfQuestion = testSubmission.Test.NumberOfQuestion;
                detailTestSubmissionDataSet.TimeLimit = (int)testSubmission.Test.TimeLimit;
                detailTestSubmissionDataSet.TestName = testSubmission.Test.Name;
                detailTestSubmissionDataSet.SubmissionDate = DateTime.Parse(detailTestSubmissionDataSet.SubmissionDate.ToString("MM/dd/yyyy h:mm tt"));

                response.Succeeded = true;
                response.Data = detailTestSubmissionDataSet;
            }
            catch (Exception ex)
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

        public async Task<IEnumerable<QuestionDataSet>> ScoringTest1()
        {
            IEnumerable<Question> datas = await _uow.QuestionRepository.Get(filter: q => q.TestId == 4);
            return datas.Select(t => _mapper.Map<QuestionDataSet>(t));
        }
    }
}
