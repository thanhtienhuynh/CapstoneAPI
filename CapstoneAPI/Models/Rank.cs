using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Rank
    {
        public int FollowingDetailId { get; set; }
        public int Position { get; set; }
        public double TotalMark { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsUpdate { get; set; }
        public int TranscriptTypeId { get; set; }

        public virtual FollowingDetail FollowingDetail { get; set; }
        public virtual TranscriptType TranscriptType { get; set; }
    }
}
