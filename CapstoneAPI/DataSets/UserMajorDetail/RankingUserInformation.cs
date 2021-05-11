using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.UserMajorDetail
{
    public class RankingUserInformationGroupByRankType
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
        public string? GroupCode { get; set; }
        public double? Position { get; set; }
        public double? TotalMark { get; set; }
    }

    public class RankingUserParam
    {
        public int UniversityId { get; set; }
        public int TrainingProgramId { get; set; }
        public int MajorId { get; set; }
    }
}
