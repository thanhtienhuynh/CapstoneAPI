using CapstoneAPI.Features.Subject.DataSet;
using System.Collections.Generic;

namespace CapstoneAPI.Features.SubjectGroup.DataSet
{
    public class SubjectGroupResponseDataSet
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public List<SubjectResponseDataSet> Subjects { get; set; }

    }
}
