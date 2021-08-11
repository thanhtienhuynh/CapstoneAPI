namespace CapstoneAPI.Features.Test.Service
{
    using AutoMapper;
    using CapstoneAPI.DataSets.Question;
    using CapstoneAPI.DataSets.Option;
    using CapstoneAPI.Helpers;
    using CapstoneAPI.Models;
    using CapstoneAPI.Repositories;
    using CapstoneAPI.Wrappers;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using Serilog;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Firebase.Storage;
    using Firebase.Auth;
    using CapstoneAPI.Filters;
    using CapstoneAPI.Filters.Test;
    using System.Linq.Expressions;
    using CapstoneAPI.Features.Test.DataSet;

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

        public async Task<Response<List<SubjectBasedTestDataSet>>> GetFilteredTests(string token, TestParam testParam)
        {
            Response<List<SubjectBasedTestDataSet>> response = new Response<List<SubjectBasedTestDataSet>>();
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
                if (subjectIds.Any())
                {

                    foreach (int subjectId in subjectIds)
                    {
                        Transcript transcript = await _uow.TranscriptRepository.GetFirst(
                        filter: t => t.UserId == user.Id && t.SubjectId == subjectId && t.Status == Consts.STATUS_ACTIVE
                                && t.TranscriptTypeId == TranscriptTypes.ThiThu);
                        double? daysRemaining = null;
                        double? userTranscript = null;
                        if (transcript != null && transcript.DateRecord != null)
                        {
                            userTranscript = transcript.Mark;
                            if (DateTime.Compare(transcript.DateRecord.Date.AddDays(90), JWTUtils.GetCurrentTimeInVN().Date) > 0)
                            {
                                daysRemaining = transcript.DateRecord.Date.AddDays(90).Subtract(JWTUtils.GetCurrentTimeInVN().Date).TotalDays;
                            }
                        }

                        IEnumerable<Test> clasifiedTests = await _uow.TestRepository
                                                    .Get(filter: test => test.Status == Consts.STATUS_ACTIVE
                                                        && test.IsSuggestedTest
                                                        && test.SubjectId == subjectId);

                        if (!clasifiedTests.Any())
                        {
                            continue;
                        }

                        Dictionary<Test, int> testSubmissionCounts = new Dictionary<Test, int>();
                        int highestCount = 0;
                        foreach(Test test in clasifiedTests)
                        {
                            int count = (await _uow.TestSubmissionRepository.Get(
                                        filter: t => t.TestId == test.Id && t.UserId == user.Id)).Count();
                            testSubmissionCounts.Add(test, count);
                            highestCount = count > highestCount ? count : highestCount;
                        }

                        Dictionary<Test, int> randomTestCounts = testSubmissionCounts.Where(t => t.Value < highestCount)
                            .ToDictionary(x => x.Key, x => x.Value);
                        List<Test> randomTests;
                        if (randomTestCounts.Any())
                        {
                            randomTests = randomTestCounts.Keys.ToList();
                        } else
                        {
                            randomTests = testSubmissionCounts.Keys.ToList();
                        }

                        Random rnd = new Random();
                        int r = rnd.Next(randomTests.Count);
                        Test clasifiedTest = randomTests[r];

                        if (clasifiedTest != null)
                        {
                            testsReponse.Add(new SubjectBasedTestDataSet()
                                {
                                    SubjectId = subjectId,
                                    Test =_mapper.Map<TestDataSet>(clasifiedTest),
                                    UniversityId = null,
                                    DaysRemaining = daysRemaining,
                                    LastTranscript = userTranscript
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

        public async Task<Response<bool>> AddNewTest(NewTestParam testParam, string token)
        {
            Response<bool> response = new Response<bool>();
            using var tran = _uow.GetTransaction();
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

                string path = Path.Combine(Path
                    .GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Configuration\TimeZoneConfiguration.json");
                JObject configuration = JObject.Parse(File.ReadAllText(path));

                foreach (var question in testParam.Questions)
                {
                    question.Result = "";
                    if (question.Options.Any())
                    {
                        foreach (var option in question.Options)
                        {
                            if (option.isResult)
                            {
                                question.Result += "1";
                            }
                            else
                            {
                                question.Result += "0";
                            }
                        }
                        if (question.Type == QuestionTypes.SingleChoice && question.Result.Count(r => r == '1') != 1)
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ!");
                            return response;
                        }
                        if (question.Type == 0 && question.Result.Count(r => r == '1') < 1)
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ!");
                            return response;
                        }
                    }
                }
                Test t = _mapper.Map<Test>(testParam);
                t.NumberOfQuestion = t.Questions.Count();
                t.UserId = user.Id;
                t.Status = Consts.STATUS_ACTIVE;
                t.IsSuggestedTest = false;
                foreach (var item in t.Questions)
                {
                    item.NumberOfOption = item.Options.Count();
                    if (item.Content != null)
                    {
                        item.Content = await FirebaseHelper.UploadBase64ImgToFirebase(item.Content);
                    }
                    foreach (var option in item.Options)
                    {
                        if (option.Content != null)
                        {
                            option.Content = await FirebaseHelper.UploadBase64ImgToFirebase(option.Content);
                        }
                    }
                    //else
                    //{
                    //    response.Succeeded = false;
                    //    if (response.Errors == null)
                    //        response.Errors = new List<string>();
                    //    response.Errors.Add("Câu hỏi số " + (item.Ordinal + 1) + " không hợp lệ!");
                    //    return response;
                    //}

                }
                //check Annotate
                foreach (var question in t.Questions)
                {
                    if (!question.IsAnnotate)
                    {
                        if (!question.Options.Any())
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Chưa có đáp án!");
                            return response;
                        }
                        List<int> optionsOrdinal = question.Options.Select(s => s.Ordinal).ToList();
                        if (optionsOrdinal.Count != optionsOrdinal.Distinct().Count())
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Thứ tự đáp án không đúng!");
                            return response;
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(question.Content))
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Nội dung không được trống!");
                            return response;
                        }
                        if (question.Options.Any())
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Dạng câu hỏi này không có đáp án!");
                            return response;
                        }
                    }
                }
                List<int> questionsOrdinal = t.Questions.Select(s => s.Ordinal).ToList();
                if (questionsOrdinal.Count != questionsOrdinal.Distinct().Count())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                        response.Errors = new List<string>();
                    response.Errors.Add("Thứ tự câu hỏi không hợp lệ!");
                    return response;
                }

                t.CreateDate = JWTUtils.GetCurrentTimeInVN();

                _uow.TestRepository.Insert(t);

                int result = await _uow.CommitAsync();
                if (result > 0)
                {
                    tran.Commit();
                    response.Succeeded = true;
                    response.Message = "Tạo mới đề thi thành công!";
                }

                else
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                        response.Errors = new List<string>();
                    response.Errors.Add("Tạo mới đề thi không thành công!");
                }
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
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<PagedResponse<List<TestPagingDataSet>>> GetTestsByFilter(PaginationFilter validFilter, TestFilter testFilter)
        {
            PagedResponse<List<TestPagingDataSet>> result = new PagedResponse<List<TestPagingDataSet>>();

            try
            {
                Expression<Func<Models.Test, bool>> filter = null;

                filter = t => (string.IsNullOrEmpty(testFilter.Name) || t.Name.Contains(testFilter.Name))
                && (testFilter.Year == null || testFilter.Year == t.Year)
                && (testFilter.SubjectId == null || testFilter.SubjectId == t.SubjectId)
                && (testFilter.TestTypeId == null || testFilter.TestTypeId == t.TestTypeId)
                && (t.Status == Consts.STATUS_ACTIVE)
                && (t.IsSuggestedTest == false);

                Func<IQueryable<Models.Test>, IOrderedQueryable<Models.Test>> order = null;
                switch (testFilter.Order)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.TestTypeId);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.TestTypeId);
                        break;
                    case 2:
                        order = order => order.OrderBy(a => a.Name);
                        break;
                    case 3:
                        order = order => order.OrderByDescending(a => a.Name);
                        break;
                    case 4:
                        order = order => order.OrderBy(a => a.Year);
                        break;
                    case 5:
                        order = order => order.OrderByDescending(a => a.Year);
                        break;
                }


                IEnumerable<Models.Test> tests = await _uow.TestRepository
                    .Get(filter: filter, orderBy: order, includeProperties: "Subject,TestType",
                    first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);

                if (tests.Count() == 0)
                {
                    result.Succeeded = true;
                    result.Message = "Không có bài thi nào!";
                }
                else
                {
                    var testPagingDataSets = new List<TestPagingDataSet>();

                    foreach (var test in tests)
                    {
                        var testPagingDataSet = _mapper.Map<TestPagingDataSet>(test);
                        testPagingDataSet.TestTypeName = test.TestType.Name;
                        testPagingDataSet.SubjectName = test.Subject.Name;
                        testPagingDataSets.Add(testPagingDataSet);
                    }
                    var totalRecords = await _uow.TestRepository.Count(filter);
                    result = PaginationHelper.CreatePagedReponse(testPagingDataSets, validFilter, totalRecords);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return result;
        }

        public async Task<Response<bool>> UpdateTestImage()
        {
            Response<bool> result = new Response<bool>();
            try
            {
                IEnumerable<Models.Question> questions = await _uow.QuestionRepository.Get(q => q.Content != null);
                foreach (var item in questions)
                {
                    item.Content = await FirebaseHelper.uploadImageLinkToFirebase(item.Content);
                }
                _uow.QuestionRepository.UpdateRange(questions);
                if (await _uow.CommitAsync() == 0)
                {
                    result.Data = false;
                }
                else
                {
                    result.Data = true;
                }
            }
            catch
            {
                result.Data = false;
            }

            return result;
        }

        public async Task<Response<bool>> UpdateTest(UpdateTestParam testParam, string token)
        {
            Response<bool> response = new Response<bool>();
            var tran = _uow.GetTransaction();
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

                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();
                Models.Test test = await _uow.TestRepository.GetFirst(
                    filter: t => t.Id == testParam.Id && t.Status == Consts.STATUS_ACTIVE, 
                    includeProperties: "Questions");
                if (test == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                        response.Errors = new List<string>();
                    response.Errors.Add("Đề kiểm tra không tồn tại");
                    return response;
                }
                if (testParam.Status == Consts.STATUS_INACTIVE)
                {
                    test.Status = testParam.Status;
                    _uow.TestRepository.Update(test);
                    if (await _uow.CommitAsync() <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                            response.Errors = new List<string>();
                        response.Errors.Add("Lỗi hệ thống");
                        return response;
                    }
                    tran.Commit();
                    response.Succeeded = true;
                    return response;
                }
                //kiểm tra ordinal
                List<int> questionsOrdinal = testParam.Questions.Select(s => s.Ordinal).ToList();
                if (questionsOrdinal.Count != questionsOrdinal.Distinct().Count())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                        response.Errors = new List<string>();
                    response.Errors.Add("Thứ tự câu hỏi không hợp lệ!");
                    return response;
                }
                //check Annotate
                foreach (var question in testParam.Questions)
                {
                    if (!question.IsAnnotate)
                    {
                        if (!question.Options.Any())
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Chưa có đáp án!");
                            return response;
                        }
                        List<int> optionsOrdinal = question.Options.Select(s => s.Ordinal).ToList();
                        if (optionsOrdinal.Count != optionsOrdinal.Distinct().Count())
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Thứ tự đáp án không đúng!");
                            return response;
                        }
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(question.Content))
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Nội dung không được trống!");
                            return response;
                        }
                        if (question.Options.Any())
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                                response.Errors = new List<string>();
                            response.Errors.Add("Câu hỏi số " + (question.Ordinal + 1) + " không hợp lệ. Dạng câu hỏi này không có đáp án!");
                            return response;
                        }
                    }
                }
                foreach (var item in testParam.Questions)
                {
                    List<int> optionsOrdinal = item.Options.Select(s => s.Ordinal).ToList();
                    if (optionsOrdinal.Count != optionsOrdinal.Distinct().Count())
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                            response.Errors = new List<string>();
                        response.Errors.Add("Thứ tự đáp án câu hỏi số " + (item.Ordinal + 1) + " không hợp lệ!");
                        return response;
                    }
                }
                //nếu xóa bài thi
                //cập nhật bài thi
                if (testParam.Status == Consts.STATUS_ACTIVE)
                {
                    Models.Test newTest = new Test
                    {
                        Name = testParam.Name,
                        IsSuggestedTest = testParam.IsSuggestedTest,
                        Level = testParam.Level,
                        Status = Consts.STATUS_ACTIVE,
                        CreateDate = currentDate,
                        SubjectId = testParam.SubjectId,
                        TestTypeId = testParam.TestTypeId,
                        UserId = user.Id,
                        UniversityId = testParam.UniversityId,
                        Year = testParam.Year,
                        TimeLimit = testParam.TimeLimit,
                        NumberOfQuestion = testParam.Questions.Count,
                    };
                    //kiểm tra có ai làm chưa
                    //Bài đã có người làm
                    if ((await _uow.TestSubmissionRepository.Get(filter: s => s.TestId == test.Id)).Any())
                    {
                        test.Status = Consts.STATUS_INACTIVE;
                        _uow.TestRepository.Update(test);
                        _uow.TestRepository.Insert(newTest);

                        if (await _uow.CommitAsync() <= 0)
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Lỗi hệ thống!");
                        }
                    }
                    else //TH2: bài chưa có người làm
                    {
                        //xóa hết câu hỏi cũ                        
                        if (test.Questions.Any())
                        {
                            foreach (var item in test.Questions)
                            {
                                _uow.OptionRepository.DeleteComposite(filter: o => o.QuestionId == item.Id);
                                _uow.QuestionRepository.Delete(item.Id);
                            }
                            if (await _uow.CommitAsync() <= 0)
                            {
                                response.Succeeded = false;
                                if (response.Errors == null)
                                {
                                    response.Errors = new List<string>();
                                }
                                response.Errors.Add("Lỗi hệ thống!");
                            }
                        }

                        newTest.Id = test.Id;
                        //cập nhật lại test information
                        test.Name = testParam.Name;
                        test.IsSuggestedTest = testParam.IsSuggestedTest;
                        test.Level = testParam.Level;
                        test.Status = Consts.STATUS_ACTIVE;
                        test.CreateDate = currentDate;
                        test.SubjectId = testParam.SubjectId;
                        test.TestTypeId = testParam.TestTypeId;
                        test.UserId = user.Id;
                        test.UniversityId = testParam.UniversityId;
                        test.Year = testParam.Year;
                        test.TimeLimit = testParam.TimeLimit;
                        test.NumberOfQuestion = testParam.Questions.Count;
                        _uow.TestRepository.Update(test);
                    }

                    //cập nhật đáp án đúng
                    foreach (var question in testParam.Questions)
                    {
                        Models.Question newQuestion = new Question
                        {
                            Content = await FirebaseHelper.UploadBase64ImgToFirebase(question.Content),
                            Ordinal = question.Ordinal,
                            Type = question.Type,
                            IsAnnotate = question.IsAnnotate,
                            TestId = newTest.Id
                        };
                        newQuestion.Result = "";
                        newQuestion.NumberOfOption = 0;
                        if (question.Options.Any())
                        {
                            foreach (var option in question.Options)
                            {
                                if (option.isResult)
                                {
                                    newQuestion.Result += "1";
                                    newQuestion.NumberOfOption++;
                                }
                                else
                                {
                                    newQuestion.Result += "0";
                                    newQuestion.NumberOfOption++;
                                }
                            }
                            if (newQuestion.Type == QuestionTypes.SingleChoice && newQuestion.Result.Count(r => r == '1') != 1)
                            {
                                response.Succeeded = false;
                                if (response.Errors == null)
                                    response.Errors = new List<string>();
                                response.Errors.Add("Câu hỏi số " + (newQuestion.Ordinal + 1) + " không hợp lệ!");
                                return response;
                            }
                            if (newQuestion.Type == 0 && newQuestion.Result.Count(r => r == '1') < 1)
                            {
                                response.Succeeded = false;
                                if (response.Errors == null)
                                    response.Errors = new List<string>();
                                response.Errors.Add("Câu hỏi số " + (newQuestion.Ordinal + 1) + " không hợp lệ!");
                                return response;
                            }

                        }
                        _uow.QuestionRepository.Insert(newQuestion);
                        if (await _uow.CommitAsync() <= 0)
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Lỗi hệ thống!");
                        }
                        if (question.Options.Any())
                        {
                            foreach (var option in question.Options)
                            {
                                Models.Option newOption = new Option
                                {
                                    Content = await FirebaseHelper.UploadBase64ImgToFirebase(option.Content),
                                    Ordinal = option.Ordinal,
                                    QuestionId = newQuestion.Id
                                };
                                _uow.OptionRepository.Insert(newOption);
                                if (await _uow.CommitAsync() <= 0)
                                {
                                    response.Succeeded = false;
                                    if (response.Errors == null)
                                    {
                                        response.Errors = new List<string>();
                                    }
                                    response.Errors.Add("Lỗi hệ thống!");
                                }
                            }
                        }
                    }

                }
                //commit ở đây
                tran.Commit();
                response.Succeeded = true;
                response.Message = "Cập nhật đề thi thành công!";
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
                response.Errors.Add("Lỗi hệ thống: " + ex.ToString());
            }
            return response;
        }

        public async Task<Response<bool>> UpdateSuggestTest(SetSuggestedTestParam setSuggestedTestParam, string token)
        {
            Response<bool> response = new Response<bool>();
            var tran = _uow.GetTransaction();
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

                Models.Test test = await _uow.TestRepository.GetById(setSuggestedTestParam.TestId);
                if (test == null && test.Status != Consts.STATUS_ACTIVE)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bài kiểm tra không tồn tại!");
                    return response;
                }
                IEnumerable<Models.Test> tests = await _uow.TestRepository.Get(
                    filter: t => t.SubjectId == test.SubjectId && t.IsSuggestedTest
                    && t.Status == Consts.STATUS_ACTIVE && t.Id != setSuggestedTestParam.TestId);

                if (!setSuggestedTestParam.IsSuggestTest && !tests.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Hệ thống phải có tối thiểu 1 bài thi thử!");
                    return response;
                }
                test.IsSuggestedTest = setSuggestedTestParam.IsSuggestTest;
                _uow.TestRepository.Update(test);
                
                if (await _uow.CommitAsync() <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lỗi hệ thống!");
                    return response;
                }
                tran.Commit();
                response.Succeeded = true;
                response.Data = true;
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
                response.Errors.Add("Lỗi hệ thống: " + ex.ToString());
            }

            return response;
        }

        public async Task<PagedResponse<List<TestAdminDataSet>>> AdminGetTestsByFilter(PaginationFilter validFilter, TestFilter testFilter)
        {
            PagedResponse<List<TestAdminDataSet>> result = new PagedResponse<List<TestAdminDataSet>>();

            try
            {
                Expression<Func<Models.Test, bool>> filter = null;

                filter = t => (string.IsNullOrEmpty(testFilter.Name) || t.Name.Contains(testFilter.Name))
                && (testFilter.Year == null || testFilter.Year == t.Year)
                && (testFilter.SubjectId == null || testFilter.SubjectId == t.SubjectId)
                && (testFilter.TestTypeId == null || testFilter.TestTypeId == t.TestTypeId)
                && (t.Status == Consts.STATUS_ACTIVE);

                Func<IQueryable<Models.Test>, IOrderedQueryable<Models.Test>> order = null;
                switch (testFilter.Order)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.TestTypeId);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.TestTypeId);
                        break;
                    case 2:
                        order = order => order.OrderBy(a => a.Name);
                        break;
                    case 3:
                        order = order => order.OrderByDescending(a => a.Name);
                        break;
                    case 4:
                        order = order => order.OrderBy(a => a.Year);
                        break;
                    case 5:
                        order = order => order.OrderByDescending(a => a.Year);
                        break;
                }


                IEnumerable<Models.Test> tests = await _uow.TestRepository
                    .Get(filter: filter, orderBy: order, includeProperties: "Subject,TestType",
                    first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);

                if (tests.Count() == 0)
                {
                    result.Succeeded = true;
                    result.Message = "Không có bài thi nào!";
                }
                else
                {
                    var testPagingDataSets = new List<TestAdminDataSet>();

                    foreach (var test in tests)
                    {
                        var testPagingDataSet = _mapper.Map<TestAdminDataSet>(test);
                        testPagingDataSet.TestTypeName = test.TestType.Name;
                        testPagingDataSet.SubjectName = test.Subject.Name;
                        testPagingDataSets.Add(testPagingDataSet);
                    }
                    var totalRecords = await _uow.TestRepository.Count(filter);
                    result = PaginationHelper.CreatePagedReponse(testPagingDataSets, validFilter, totalRecords);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return result;
        }
    }
}
