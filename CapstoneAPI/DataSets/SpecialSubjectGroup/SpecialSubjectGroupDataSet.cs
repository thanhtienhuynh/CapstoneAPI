using CapstoneAPI.Features.Subject.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.SpecialSubjectGroup
{
    public class SpecialSubjectGroupDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public List<SubjectDataSet> Subjects { get; set; }
    }
}
