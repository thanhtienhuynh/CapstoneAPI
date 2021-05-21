﻿using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class EntryMark
    {
        public int Id { get; set; }
        public int MajorSubjectGroupId { get; set; }
        public double? Mark { get; set; }
        public int SubAdmissionCriterionId { get; set; }

        public virtual MajorSubjectGroup MajorSubjectGroup { get; set; }
        public virtual SubAdmissionCriterion SubAdmissionCriterion { get; set; }
    }
}
