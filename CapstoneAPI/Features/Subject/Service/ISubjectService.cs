using CapstoneAPI.Features.Subject.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Subject.Service
{
    public interface ISubjectService
    {
        Task<Response<IEnumerable<SubjectDataSet>>> GetAllSubjects();
    }
}
