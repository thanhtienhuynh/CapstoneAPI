using CapstoneAPI.Features.Test.DataSet;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Test;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Test.Service
{
    public interface ITestService
    {
        Task<Response<List<SubjectBasedTestDataSet>>> GetFilteredTests(TestParam testParam);
        Task<Response<TestDataSet>> GetTestById(int id);
        Task<Response<bool>> AddNewTest(NewTestParam testParam, string token);
        Task<PagedResponse<List<TestPagingDataSet>>> GetTestsByFilter(PaginationFilter validFilter, TestFilter testFilter);
        Task<Response<bool>> UpdateTestImage();
        Task<Response<bool>> UpdateTest(UpdateTestParam testParam, string token);
        Task<Response<bool>> UpdateSuggestTest(SetSuggestedTestParam setSuggestedTestParam, string token);
        Task<PagedResponse<List<TestAdminDataSet>>> AdminGetTestsByFilter(PaginationFilter validFilter, TestFilter testFilter);
    }
}
