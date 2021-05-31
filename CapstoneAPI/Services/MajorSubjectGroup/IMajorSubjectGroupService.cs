using CapstoneAPI.DataSets.MajorSubjectGroup;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.MajorSubjectGroup
{
    public interface IMajorSubjectGroupService
    {
        Task<Response<IEnumerable<MajorSubjectGroupDataSet>>> GetMajorSubjectGourpByMajor(int majorId);
    }
}
