﻿using CapstoneAPI.Features.Subject.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.SubjectGroup.DataSet
{
    public class CreateSubjectGroupDataset
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int Status { get; set; }
        public IEnumerable<SubjectDataSet> ListOfSubject { get; set; }
    }
}
