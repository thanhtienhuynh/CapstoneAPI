﻿using CapstoneAPI.DataSets.Subject;
using System.Collections.Generic;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class SubjectGroupWeightDataSet
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public List<SubjectWeightDataSet> SubjectWeights { get; set; }
    }
}
