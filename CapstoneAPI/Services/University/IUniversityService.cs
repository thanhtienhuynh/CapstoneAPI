using CapstoneAPI.DataSets.University;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.MajorDetail;
using CapstoneAPI.Filters.University;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.University
{
    public interface IUniversityService
    {
        Task<Response<IEnumerable<TrainingProgramBasedUniversityDataSet>>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam, string token);
        Task<Response<MockTestBasedUniversity>> CalculaterUniversityByMockTestMarks(MockTestsUniversityParam universityParam, string token);
        Task<PagedResponse<List<AdminUniversityDataSet>>> GetUniversities(PaginationFilter validFilter, UniversityFilter universityFilter);
        Task<PagedResponse<List<UniMajorDataSet>>> GetDetailUniversity(PaginationFilter validFilter, MajorDetailFilter majorDetailFilter);
        Task<Response<AdminUniversityDataSet>> CreateNewAnUniversity(CreateUniversityDataset createUniversityDataset);
        Task<Response<AdminUniversityDataSet>> UpdateUniversity(AdminUniversityDataSet adminUniversityDataSet);
        Task<Response<bool>> AddMajorToUniversity(AddingMajorUniversityParam addingMajorUniversityParam);
        Task<Response<bool>> UpdateMajorOfUniversity(UpdatingMajorUniversityParam updatingMajorUniversityParam);
        Task<Response<bool>> DeleteMajorOfUnivesity(int majorDetailId);
    }
}
