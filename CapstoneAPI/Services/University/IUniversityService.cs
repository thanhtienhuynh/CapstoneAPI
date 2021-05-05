using CapstoneAPI.DataSets.University;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.University
{
    public interface IUniversityService
    {
        Task<Response<IEnumerable<UniversityDataSetBaseOnTrainingProgram>>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam, string token);
        Task<IEnumerable<AdminUniversityDataSet>> GetUniversities();
        Task<DetailUniversityDataSet> GetDetailUniversity(int universityId);
        Task<AdminUniversityDataSet> CreateNewAnUniversity(CreateUniversityDataset createUniversityDataset);
        Task<AdminUniversityDataSet> UpdateUniversity(AdminUniversityDataSet adminUniversityDataSet);
        Task<DetailUniversityDataSet> AddMajorToUniversity(AddingMajorUniversityParam addingMajorUniversityParam);
        Task<DetailUniversityDataSet> UpdateMajorOfUniversity(UpdatingMajorUniversityParam updatingMajorUniversityParam);
    }
}
