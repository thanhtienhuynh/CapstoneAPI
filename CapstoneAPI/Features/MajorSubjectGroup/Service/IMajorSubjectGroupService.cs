using CapstoneAPI.Features.MajorSubjectGroup.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.MajorSubjectGroup.Service
{
    public interface IMajorSubjectGroupService
    {
        Task<Response<IEnumerable<MajorSubjectGroupDataSet>>> GetMajorSubjectGourpByMajor(int majorId);
        Task<Response<MajorSubjectGroupDataSet>> AddAMajorSubjectGroup(MajorSubjectGroupParam majorSubjectGroupParam);

    }
}
