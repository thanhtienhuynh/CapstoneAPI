namespace CapstoneAPI.Services.Test
{
    using AutoMapper;
    using CapstoneAPI.DataSets.Question;
    using CapstoneAPI.DataSets.Option;
    using CapstoneAPI.DataSets.Test;
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

                if (userIdString == null || userIdString.Length <= 0)
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
                        if (question.Type == 1 && question.Result.Count(r => r == '1') != 1)
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
                t.UserId = userId;
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

                var currentTimeZone = configuration.SelectToken("CurrentTimeZone").ToString();
                DateTime currentDate = DateTime.UtcNow.AddHours(double.Parse(currentTimeZone));
                t.CreateDate = currentDate;


                _uow.TestRepository.Insert(t);

                int result = await _uow.CommitAsync();
                if (result > 0)
                {
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
                    var totalRecords = _uow.TestRepository.Count(filter);
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
    }
}
