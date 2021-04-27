using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class UserMajorDetail
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MajorDetailId { get; set; }
        public int Status { get; set; }

        public virtual MajorDetail MajorDetail { get; set; }
        public virtual User User { get; set; }
        public virtual Rank Rank { get; set; }
    }
}
