using CapstoneAPI.DataSets.University;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.University
{
    public interface IUniversityService
    {
        Task<IEnumerable<UniversityDataSet>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam);
    }
}
