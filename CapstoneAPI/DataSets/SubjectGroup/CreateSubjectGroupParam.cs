using CapstoneAPI.DataSets.Subject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class CreateSubjectGroupParam
    {
        public string GroupCode { get; set; }
        public List<int?> SubjectIds { get; set; }
        public List<int?> SpecicalSubjectGroupIds { get;set; }
    }
}
