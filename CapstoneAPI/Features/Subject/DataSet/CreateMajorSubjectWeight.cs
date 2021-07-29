using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Subject.DataSet
{
    public class CreateMajorSubjectWeight
    {
        public int SubjectId { get; set; }
        public int Weight { get; set; }
        public bool IsSpecialSubjectGroup { get; set; }
    }
}
