using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Subject
{
    public interface ISubjectService
    {
        Task<Response<IEnumerable<SubjectDataSet>>> GetAllSubjects();
    }
}
