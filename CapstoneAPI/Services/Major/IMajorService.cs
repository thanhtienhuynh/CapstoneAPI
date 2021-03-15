using CapstoneAPI.DataSets.Major;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Major
{
    public interface IMajorService
    {
        Task<IEnumerable<AdminMajorDataSet>> GetActiveMajorsByAdmin();
    }
}
