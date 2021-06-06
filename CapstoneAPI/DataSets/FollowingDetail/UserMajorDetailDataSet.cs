using CapstoneAPI.DataSets.Rank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.FollowingDetail
{
    public class UserMajorDetailDataSet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MajorDetailId { get; set; }
        public int Status { get; set; }

        public Models.MajorDetail MajorDetail { get; set; }
        public Models.User User { get; set; }
        public RankDataSet Rank { get; set; }
    }
}
