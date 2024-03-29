﻿using CapstoneAPI.Features.SubjectGroup.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.University.DataSet
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

    public class MajorDetailParam
    {
        public int UniversityId { get; set; }
        public int SeasonId { get; set;}
    }
  
    public class MockTestsUniversityParam
    {
        public int SubjectGroupId { get; set; }
        public int MajorId { get; set; }
    }
}
