using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Rank
    {
        public int Id { get; set; }
        public int RankTypeId { get; set; }
        public int Position { get; set; }
        public int UserMajorDetailId { get; set; }
        public DateTime UpdatedDate { get; set; }

        public virtual RankType RankType { get; set; }
        public virtual UserMajorDetail UserMajorDetail { get; set; }
    }
}
