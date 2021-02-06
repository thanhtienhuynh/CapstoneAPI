using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class SubjectGroupDetail
    {
        public int SubjectId { get; set; }
        public int SubjectGroupId { get; set; }

        public virtual Subject Subject { get; set; }
        public virtual SubjectGroup SubjectGroup { get; set; }
    }
}
