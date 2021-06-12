using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Rank
{
    public class RankDataSet
    {
        public int FollowingDetailId { get; set; }
        public int TranscriptTypeId { get; set; }
        public int Position { get; set; }
        public int NewPosition { get; set; }
        public double TotalMark { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsReceiveNotification { get; set; }
        public bool IsUpdate { get; set; }
    }
}
