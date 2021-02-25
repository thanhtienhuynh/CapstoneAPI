using CapstoneAPI.DataSets.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Test
{
    public interface ITestService
    {
        Task<IEnumerable<TestDataSet>> GetFilteredTests(TestParam testParam);
        Task<TestDataSet> GetTestById(int id);
    }
}
