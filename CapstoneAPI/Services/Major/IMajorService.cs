using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Filters;
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
        Task<Response<List<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(string majorName);
        Task<PagedResponse<List<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(PaginationFilter validFilter, string majorName);
        Task<Response<CreateMajorSubjectWeightDataSet>> CreateAMajor(CreateMajorSubjectWeightDataSet createMajor);

    }
}
