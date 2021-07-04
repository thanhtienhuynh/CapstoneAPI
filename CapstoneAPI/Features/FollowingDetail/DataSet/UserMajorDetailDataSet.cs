using CapstoneAPI.Features.Rank.DataSet;
using CapstoneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.FollowingDetail.DataSet
{
    public class RankFollowingDetailDataSet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MajorDetailId { get; set; }

        public EntryMark EntryMark { get; set; }
        public Models.User User { get; set; }
        public RankDataSet RankDataSet { get; set; }
    }
}
