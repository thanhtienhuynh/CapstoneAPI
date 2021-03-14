using CapstoneAPI.DataSets.SubjectGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.SubjectGroup
{
    public interface ISubjectGroupService
    {
        Task<IEnumerable<SubjectGroupDataSet>> GetCaculatedSubjectGroup(SubjectGroupParam subjectGroupParam);
        Task<IEnumerable<AdminSubjectGroupDataSet>> GetListSubjectGroups();
    }
}
