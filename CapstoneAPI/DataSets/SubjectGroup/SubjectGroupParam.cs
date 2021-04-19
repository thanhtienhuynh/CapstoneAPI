using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class SubjectGroupParam
    {
        public List<MarkParam> Marks { get; set; }
        public int TranscriptTypeId { get; set; }
    }

    public class MarkParam
    {
        public int SubjectId { get; set; }
        public double Mark { get; set; }
    }
}
