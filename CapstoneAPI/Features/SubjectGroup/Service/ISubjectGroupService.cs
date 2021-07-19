using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.SubjectGroup.Service
{
    public interface ISubjectGroupService
    {
        Task<Response<IEnumerable<SubjectGroupDataSet>>> GetCaculatedSubjectGroup(SubjectGroupParam subjectGroupParam, string token);
        Task<Response<UserSuggestionInformation>> GetUserSuggestTopSubjectGroup(string token);
        Task<Response<IEnumerable<AdminSubjectGroupDataSet>>> GetListSubjectGroups();
        Task<Response<CreateSubjectGroupDataset>> CreateNewSubjectGroup(CreateSubjectGroupParam createSubjectGroupParam);
        Task<Response<CreateSubjectGroupDataset>> UpdateSubjectGroup(UpdateSubjectGroupParam updateSubjectGroupParam);
        Task<Response<SubjectGroupResponseDataSet>> GetSubjectGroupWeight(int id);
    }
}
