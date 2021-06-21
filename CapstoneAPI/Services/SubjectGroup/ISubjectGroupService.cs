using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.SubjectGroup
{
    public interface ISubjectGroupService
    {
        Task<Response<IEnumerable<SubjectGroupDataSet>>> GetCaculatedSubjectGroup(SubjectGroupParam subjectGroupParam);
        Task<Response<UserSuggestionInformation>> GetUserSuggestTopSubjectGroup(string token);
        Task<Response<IEnumerable<AdminSubjectGroupDataSet>>> GetListSubjectGroups();
        Task<Response<CreateSubjectGroupDataset>> CreateNewSubjectGroup(CreateSubjectGroupParam createSubjectGroupParam);
        Task<Response<CreateSubjectGroupDataset>> UpdateSubjectGroup(UpdateSubjectGroupParam updateSubjectGroupParam);
        Task<Response<SubjectGroupResponseDataSet>> GetSubjectGroupWeight(int id);
    }
}
