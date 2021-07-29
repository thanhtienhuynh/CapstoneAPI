using CapstoneAPI.Features.TestType.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.TestType.Service
{
    public interface ITestTypeService
    {
        Task<Response<IEnumerable<TestTypeDataSet>>> GetAllTestTypes();
    }
}
