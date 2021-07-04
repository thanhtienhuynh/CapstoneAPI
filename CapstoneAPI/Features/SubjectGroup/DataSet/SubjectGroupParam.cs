using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.SubjectGroup.DataSet
{
    public class SubjectGroupParam
    {
        public List<MarkParam> Marks { get; set; }
        public int TranscriptTypeId { get; set; }
        public int ProvinceId { get; set; }
        public int Gender { get; set; }
    }

    public class MarkParam
    {
        public int SubjectId { get; set; }
        public double Mark { get; set; }
    }
}
