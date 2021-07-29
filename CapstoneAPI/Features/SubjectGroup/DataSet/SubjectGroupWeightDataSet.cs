using CapstoneAPI.Features.Subject.DataSet;
using System.Collections.Generic;

namespace CapstoneAPI.Features.SubjectGroup.DataSet
{
    public class SubjectGroupWeightDataSet
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public List<SubjectWeightDataSet> SubjectWeights { get; set; }
    }
}
