using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.FollowingDetail
{
    public class RankingUserInformationGroupByTranscriptType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<RankingUserInformation> RankingUserInformations { get; set; }
    }
    public class RankingUserInformation
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public string GroupCode { get; set; }
        public int? Position { get; set; }
        public double? TotalMark { get; set; }
    }
}
