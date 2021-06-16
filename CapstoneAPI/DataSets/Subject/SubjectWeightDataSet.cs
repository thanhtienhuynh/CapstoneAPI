﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Subject
{
    public class SubjectWeightDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Weight { get; set; }
        public bool IsSpecialSubjectGroup { get; set; }
    }
}
