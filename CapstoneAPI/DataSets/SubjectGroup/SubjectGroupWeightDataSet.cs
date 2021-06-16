using CapstoneAPI.DataSets.Subject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class SubjectGroupWeightDataSet
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public List<SubjectWeightDataSet> SubjectWeights { get; set; }
    }
}
