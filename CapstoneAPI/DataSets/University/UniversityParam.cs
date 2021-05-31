using CapstoneAPI.DataSets.SubjectGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.University
{
    public class UniversityParam
    {
        public int SubjectGroupId { get; set; }
        public int MajorId { get; set; }
        public double TotalMark { get; set; }
        public int TranscriptTypeId { get; set; }
        public int? ProvinceId { get; set; }
        public int? Gender { get; set; }
    }

    public class MockTestsUniversityParam
    {
        public int SubjectGroupId { get; set; }
        public int MajorId { get; set; }
        public int TranscriptTypeId { get; set; }
        public int? ProvinceId { get; set; }
        public int? Gender { get; set; }
        public List<MarkParam> Marks { get; set; }
    }
}
