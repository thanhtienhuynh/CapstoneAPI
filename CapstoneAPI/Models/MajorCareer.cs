using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class MajorCareer
    {
        public int MajorId { get; set; }
        public int CareerId { get; set; }

        public virtual Career Career { get; set; }
        public virtual Major Major { get; set; }
    }
}
