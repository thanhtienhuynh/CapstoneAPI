﻿using CapstoneAPI.Features.SubjectGroup.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Major.DataSet
{
    public class MajorSubjectWeightDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        public List<SubjectGroupWeightDataSet> SubjectGroups { get; set; }
    }
}