using CapstoneAPI.DataSets.Subject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Subject
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectDataSet>> GetAllSubjects();
    }
}
