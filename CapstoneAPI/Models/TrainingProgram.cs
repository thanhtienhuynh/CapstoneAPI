﻿using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class TrainingProgram
    {
        public TrainingProgram()
        {
            MajorDetails = new HashSet<MajorDetail>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }

        public virtual ICollection<MajorDetail> MajorDetails { get; set; }
    }
}
