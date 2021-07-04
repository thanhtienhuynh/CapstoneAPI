using CapstoneAPI.DataSets.SubjectGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Major
{
    public class CreateMajorSubjectWeightDataSet
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Curriculum { get; set; }
        public string HumanQuality { get; set; }
        public string SalaryDescription { get; set; }
        public List<CreateMajorSubjectGroup> SubjectGroups { get; set; }
    }
}
