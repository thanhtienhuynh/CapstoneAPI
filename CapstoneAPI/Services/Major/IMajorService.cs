using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.University;
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
        Task<Response<ResultOfCreateMajorDataSet>> CreateAMajor(CreateMajorDataSet createMajorDataSet);
        Task<Response<ResultOfCreateMajorDataSet>> UpdateAMajor(ResultOfCreateMajorDataSet updateMajor);
        Task<PagedResponse<List<NumberUniversityInMajorDataSet>>> GetNumberUniversitiesInMajor(PaginationFilter validFilter, MajorToNumberUniversityFilter majorToUniversityFilter);
        Task<Response<List<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(string majorName);
        Task<PagedResponse<List<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(PaginationFilter validFilter, string majorName);
        Task<Response<CreateMajorSubjectWeightDataSet>> CreateAMajor(CreateMajorSubjectWeightDataSet createMajor);
        Task<Response<UpdateMajorParam>> UpdateMajor(UpdateMajorParam updateMajor);
        Task<Response<UpdateMajorParam2>> UpdateMajor(UpdateMajorParam2 updateMajor);
        Task<Response<MajorDetailDataSet>> GetUniversitiesInMajor(int majorId);
    }
}
