using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class UpdateSubjectGroupParam
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }

        public int Status { get; set; }
        public List<int> ListOfSubjectId { get; set; }
    }
}
