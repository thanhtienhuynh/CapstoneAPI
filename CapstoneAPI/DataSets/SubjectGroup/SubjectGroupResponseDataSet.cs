using CapstoneAPI.DataSets.Subject;
using System.Collections.Generic;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class SubjectGroupResponseDataSet
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public List<SubjectResponseDataSet> Subjects { get; set; }

    }
}
