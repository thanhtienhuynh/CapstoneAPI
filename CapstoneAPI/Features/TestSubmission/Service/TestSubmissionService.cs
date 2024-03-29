﻿namespace CapstoneAPI.Features.TestSubmission.Service
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
                if (loadedTest == null || loadedTest.Status != Consts.STATUS_ACTIVE)
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
                    SubmissionDate = JWTUtils.GetCurrentTimeInVN(),
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

            User user = await _uow.UserRepository.GetUserByToken(token);

            if (user == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            using var tran = _uow.GetTransaction();
            try
            {
                foreach (SaveTestSubmissionParam saveTestSubmissionParam in saveTestSubmissionParams)
                {
                    Test test = await _uow.TestRepository.GetFirst(filter: t => t.Id == saveTestSubmissionParam.TestId
                                                                && t.Status == Consts.STATUS_ACTIVE);
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
                    TestSubmission testSubmission = null;
                    if (saveTestSubmissionParam.TestSubmissionId == null)
                    {
                        testSubmission = new TestSubmission()
                        {
                            TestId = saveTestSubmissionParam.TestId,
                            SpentTime = saveTestSubmissionParam.SpentTime,
                            SubmissionDate = JWTUtils.GetCurrentTimeInVN(),
                            NumberOfRightAnswers = saveTestSubmissionParam.NumberOfRightAnswers,
                            Mark = Math.Round(saveTestSubmissionParam.Mark, 2),
                            UserId = user.Id
                        };
                        _uow.TestSubmissionRepository.Insert(testSubmission);
                    }
                    else
                    {
                        testSubmission = await _uow.TestSubmissionRepository.GetById(saveTestSubmissionParam.TestSubmissionId);
                        if (testSubmission != null)
                        {
                            testSubmission.SpentTime = saveTestSubmissionParam.SpentTime;
                            testSubmission.SubmissionDate = JWTUtils.GetCurrentTimeInVN();
                            testSubmission.Mark = Math.Round(saveTestSubmissionParam.Mark, 2);
                            testSubmission.NumberOfRightAnswers = saveTestSubmissionParam.NumberOfRightAnswers;
                            _uow.TestSubmissionRepository.Update(testSubmission);
                        } else
                        {
                            testSubmission = new TestSubmission()
                            {
                                TestId = saveTestSubmissionParam.TestId,
                                SpentTime = saveTestSubmissionParam.SpentTime,
                                SubmissionDate = JWTUtils.GetCurrentTimeInVN(),
                                NumberOfRightAnswers = saveTestSubmissionParam.NumberOfRightAnswers,
                                Mark = Math.Round(saveTestSubmissionParam.Mark, 2),
                                UserId = user.Id
                            };
                            _uow.TestSubmissionRepository.Insert(testSubmission);
                        }
                    }

                    int subjectId = (int) test.SubjectId;
                    
                    if (test.IsSuggestedTest)
                    {
                        IEnumerable<Transcript> transcripts = await _uow.TranscriptRepository
                                .Get(t => t.TranscriptTypeId == TranscriptTypes.ThiThu && t.UserId == user.Id
                                && t.SubjectId == subjectId && t.Status == Consts.STATUS_ACTIVE);

                        if (transcripts.Any())
                        {
                            foreach (Transcript transcript in transcripts)
                            {
                                transcript.DateRecord = JWTUtils.GetCurrentTimeInVN();
                                transcript.IsUpdate = false;
                                transcript.Status = Consts.STATUS_INACTIVE;
                            }
                            _uow.TranscriptRepository.UpdateRange(transcripts);
                        }

                        _uow.TranscriptRepository.Insert(new Transcript()
                        {
                            UserId = user.Id,
                            DateRecord = JWTUtils.GetCurrentTimeInVN(),
                            Mark = Math.Round(saveTestSubmissionParam.Mark, 2),
                            TranscriptTypeId = TranscriptTypes.ThiThu,
                            SubjectId = subjectId,
                            IsUpdate = true,
                            Status = Consts.STATUS_ACTIVE
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

        public async Task<Response<int>> SaveFirstTestSubmission(FirstTestSubmissionParam saveTestSubmissionParam, string token)
        {
            Response<int> response = new Response<int>();
            using var tran = _uow.GetTransaction();
            try
            {
                User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                Test test = await _uow.TestRepository.GetFirst(filter: t => t.Id == saveTestSubmissionParam.TestId
                                                                && t.Status == Consts.STATUS_ACTIVE);
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

                TestSubmission testSubmission = new TestSubmission()
                {
                    TestId = saveTestSubmissionParam.TestId,
                    SpentTime = 0,
                    SubmissionDate = JWTUtils.GetCurrentTimeInVN(),
                    NumberOfRightAnswers = 0,
                    Mark = 0,
                    UserId = user.Id
                };

                IEnumerable<Transcript> transcripts = await _uow.TranscriptRepository
                                            .Get(filter: t => t.SubjectId == test.SubjectId
                                                        && t.UserId == user.Id
                                                        && t.TranscriptTypeId == TranscriptTypes.ThiThu
                                                        && t.Status == Consts.STATUS_ACTIVE);
                if (transcripts.Any())
                {
                    foreach (Transcript transcript in transcripts)
                    {
                        transcript.Status = Consts.STATUS_INACTIVE;
                        transcript.DateRecord = JWTUtils.GetCurrentTimeInVN();
                    }
                    _uow.TranscriptRepository.UpdateRange(transcripts);
                }

                Transcript newTranscript = new Transcript()
                {
                    Mark = 0,
                    SubjectId = (int) test.SubjectId,
                    UserId = user.Id,
                    TranscriptTypeId = TranscriptTypes.ThiThu,
                    DateRecord = JWTUtils.GetCurrentTimeInVN(),
                    IsUpdate = true,
                    Status = Consts.STATUS_ACTIVE
                };

                _uow.TranscriptRepository.Insert(newTranscript);
                _uow.TestSubmissionRepository.Insert(testSubmission);

                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lưu không thành công, lỗi hệ thống!");
                }
                
                tran.Commit();
                response.Succeeded = true;
                response.Data = testSubmission.Id;
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
            }
            return response;
        }

        public async Task<Response<List<UserTestSubmissionDataSet>>> GetTestSubmissionsByUser(string token, UserTestSubmissionQueryParam param)
        {
            Response<List<UserTestSubmissionDataSet>> response = new Response<List<UserTestSubmissionDataSet>>();
            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                    return response;
                }

                IEnumerable<TestSubmission> testSubmissionDataSets = (await _uow.TestSubmissionRepository
                    .Get(filter: t => t.UserId == user.Id,
                        includeProperties: "Test",
                        orderBy: t => t.OrderBy(t => t.SubmissionDate))).GroupBy(t => t.TestId).Select(g => g.Last());
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
                switch (param.Order ?? 1)
                {
                    case 1:
                        testSubmissionDataSets = testSubmissionDataSets.OrderByDescending(a => a.SubmissionDate);
                        break;
                    case 2:
                        testSubmissionDataSets = testSubmissionDataSets.OrderBy(a => a.SubmissionDate);
                        break;
                    case 3:
                        testSubmissionDataSets = testSubmissionDataSets.OrderByDescending(a => a.Test.Year);
                        break;
                    case 4:
                        testSubmissionDataSets = testSubmissionDataSets.OrderBy(a => a.Test.Year);
                        break;
                    case 5:
                        testSubmissionDataSets = testSubmissionDataSets.OrderByDescending(a => a.Test.Name);
                        break;
                    case 6:
                        testSubmissionDataSets = testSubmissionDataSets.OrderBy(a => a.Test.Name);
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
                        .Get(filter: t => t.UserId == user.Id && t.TestId == testSubmission.TestId)).Count();
                    userTestSubmissionDataSet.TestName = testSubmission.Test.Name;
                    userTestSubmissionDataSet.SubmissionDate = DateTime.Parse(userTestSubmissionDataSet.SubmissionDate.ToString("MM/dd/yyyy h:mm tt"));
                    userTestSubmissionDataSet.IsSuggestedTest = testSubmission.Test.IsSuggestedTest;
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
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                    return response;
                }
                TestSubmission testSubmission = await _uow.TestSubmissionRepository
                   .GetFirst(filter: t => t.Id == testSubmissionId && t.UserId == user.Id,
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
                            .Get(filter: t => t.UserId == user.Id && t.TestId == testSubmission.TestId)).Count();
                detailTestSubmissionDataSet.QuestionSubmissions = questionSubmissionDataSets;
                detailTestSubmissionDataSet.NumberOfQuestion = testSubmission.Test.NumberOfQuestion;
                detailTestSubmissionDataSet.TimeLimit = (int)testSubmission.Test.TimeLimit;
                detailTestSubmissionDataSet.TestName = testSubmission.Test.Name;
                detailTestSubmissionDataSet.SubmissionDate = DateTime.Parse(detailTestSubmissionDataSet.SubmissionDate.ToString("MM/dd/yyyy h:mm tt"));
                detailTestSubmissionDataSet.IsSuggestedTest = testSubmission.Test.IsSuggestedTest;
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

        public async Task<IEnumerable<QuestionDataSet>> ScoringTest1(int testId)
        {
            IEnumerable<Question> datas = await _uow.QuestionRepository.Get(filter: q => q.TestId == testId,
                    orderBy: q => q.OrderBy(q => q.Ordinal));
            return datas.Select(t => _mapper.Map<QuestionDataSet>(t));
        }
    }
}
