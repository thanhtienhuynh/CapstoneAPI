﻿using CapstoneAPI.DataSets.University;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Major
{
    public class NumberUniversityInMajorDataSet
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? NumberOfUniversity { get; set; }
    }
}
