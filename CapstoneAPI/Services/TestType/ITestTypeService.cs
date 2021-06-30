using CapstoneAPI.DataSets.TestType;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.TestType
{
    public interface ITestTypeService
    {
        Task<Response<IEnumerable<TestTypeDataSet>>> GetAllTestTypes();
    }
}
