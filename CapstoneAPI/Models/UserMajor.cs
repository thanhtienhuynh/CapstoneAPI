using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class UserMajor
    {
        public int UserId { get; set; }
        public int MajorId { get; set; }
        public int Status { get; set; }

        public virtual Major Major { get; set; }
        public virtual User User { get; set; }
    }
}
