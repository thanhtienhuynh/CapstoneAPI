using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.SubjectGroup.DataSet
{
    public class UpdateSubjectGroupParam
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int Status { get; set; }
        public List<SubjectGroupDetailParam> ListOfSubject { get; set; }
    }
    public class SubjectGroupDetailParam
    {
        public int SubjectId { get; set; }
        public bool IsSpecicSubject { get; set; }
    }
}
