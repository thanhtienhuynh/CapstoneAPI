using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Question;
using CapstoneAPI.DataSets.TestSubmission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.TestSubmission
{
    public interface ITestSubmissionService
    {
        Task<TestSubmissionDataSet> ScoringTest(TestSubmissionParam testSubmissionParam);
        Task<BaseResponse> SaveTestSubmission(SaveTestSubmissionParam saveTestSubmissionParam, string token);
        Task<List<UserTestSubmissionDataSet>> GetTestSubmissionsByUser(string token);
        Task<DetailTestSubmissionDataSet> GetDetailTestSubmissionByUser(int testSubmissionId, string token);
        Task<IEnumerable<QuestionDataSet>> ScoringTest1();
    }
}
