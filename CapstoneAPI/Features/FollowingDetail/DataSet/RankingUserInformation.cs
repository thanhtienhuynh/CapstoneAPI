using CapstoneAPI.Features.Major.DataSet;
using CapstoneAPI.Features.TrainingProgram.DataSet;
using CapstoneAPI.Features.University.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.FollowingDetail.DataSet
{
    public class UserFollowingDetail
    {
        public AdminTrainingProgramDataSet TrainingProgramDataSet { get; set; }
        public AdminMajorDataSet MajorDataSet { get; set; }
        public DetailUniversityDataSet UniversityDataSet { get; set; }
        public RankingInformation RankingInformation { get; set; }
        public List<RankingUserInformationGroupByTranscriptType> RankingUserInformationsGroupByTranscriptType { get; set; }
    }

    public class RankingInformation
    {
        public List<SeasonDataSet> SeasonDataSets { get; set; }
        public int? PositionOfUser { get; set; }
        public int TotalUserCared { get; set; }
        public int SubjectGroupId { get; set; }
        public string SubjectGroupCode { get; set; }
        public double? RankingMark { get; set; }
    }

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
