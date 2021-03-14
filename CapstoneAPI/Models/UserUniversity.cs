using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class UserUniversity
    {
        public int UserId { get; set; }
        public int UniversityId { get; set; }

        public virtual University University { get; set; }
        public virtual User User { get; set; }
    }
}
