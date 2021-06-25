using CapstoneAPI.DataSets.Test;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Test;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Test
{
    public interface ITestService
    {
        Task<Response<List<SubjectBasedTestDataSet>>> GetFilteredTests(TestParam testParam);
        Task<Response<TestDataSet>> GetTestById(int id);
        Task<Response<bool>> AddNewTest(NewTestParam testParam, string token);
        Task<PagedResponse<List<TestPagingDataSet>>> GetTestsByFilter(PaginationFilter validFilter, TestFilter testFilter);
        Task<Response<bool>> UpdateTestImage();
    }
}
