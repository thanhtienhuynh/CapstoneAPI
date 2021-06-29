using CapstoneAPI.DataSets.Major;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Major;
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
        Task<Response<IEnumerable<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(string majorName);
        Task<Response<ResultOfCreateMajorDataSet>> CreateAMajor(CreateMajorDataSet createMajorDataSet);
        Task<Response<ResultOfCreateMajorDataSet>> UpdateAMajor(ResultOfCreateMajorDataSet updateMajor);
        Task<PagedResponse<List<MajorToUniversityDataSet>>> GetUniversitiesInMajor(PaginationFilter validFilter, MajorToUniversityFilter majorToUniversityFilter);
    }
}
