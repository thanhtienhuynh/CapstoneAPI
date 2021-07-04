using CapstoneAPI.Features.Subject.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.SubjectGroup.DataSet
{
    public class CreateMajorSubjectGroup
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public List<CreateMajorSubjectWeight> SubjectWeights { get; set; }
    }
}
