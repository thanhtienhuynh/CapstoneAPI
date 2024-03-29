﻿using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.Features.TestSubmission.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.TestSubmission.Service
{
    public interface ITestSubmissionService
    {
        Task<Response<TestSubmissionDataSet>> ScoringTest(TestSubmissionParam testSubmissionParam);
        Task<Response<bool>> SaveTestSubmissions(List<SaveTestSubmissionParam> saveTestSubmissionParams, string token);
        Task<Response<int>> SaveFirstTestSubmission(FirstTestSubmissionParam firstTestSubmissionParam, string token);
        Task<Response<List<UserTestSubmissionDataSet>>> GetTestSubmissionsByUser(string token, UserTestSubmissionQueryParam param);
        Task<Response<DetailTestSubmissionDataSet>> GetDetailTestSubmissionByUser(int testSubmissionId, string token);
        Task<IEnumerable<QuestionDataSet>> ScoringTest1(int testId);
    }
}
