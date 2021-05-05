using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.DataSets.TestSubmission;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.TestSubmission
{
    public interface ITestSubmissionService
    {
        Task<Response<TestSubmissionDataSet>> ScoringTest(TestSubmissionParam testSubmissionParam);
        Task<Response<Models.TestSubmission>> SaveTestSubmission(SaveTestSubmissionParam saveTestSubmissionParam, string token);
        Task<Response<List<UserTestSubmissionDataSet>>> GetTestSubmissionsByUser(string token);
        Task<Response<DetailTestSubmissionDataSet>> GetDetailTestSubmissionByUser(int testSubmissionId, string token);
        Task<IEnumerable<QuestionDataSet>> ScoringTest1();
    }
}
