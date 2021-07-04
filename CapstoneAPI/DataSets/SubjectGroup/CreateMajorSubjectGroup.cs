using CapstoneAPI.DataSets.Subject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class CreateMajorSubjectGroup
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public List<CreateMajorSubjectWeight> SubjectWeights { get; set; }
    }
}
