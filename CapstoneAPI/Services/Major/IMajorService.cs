using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Major
{
    public interface IMajorService
    {
        Task<Response<IEnumerable<AdminMajorDataSet>>> GetActiveMajorsByAdmin();
        Task<Response<ResultOfCreateMajorDataSet>> CreateAMajor(CreateMajorDataSet createMajorDataSet);
        Task<Response<ResultOfCreateMajorDataSet>> UpdateAMajor(ResultOfCreateMajorDataSet updateMajor);
    }
}
