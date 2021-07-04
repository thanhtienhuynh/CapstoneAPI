using CapstoneAPI.Features.University.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.FollowingDetail.DataSet
{
    public class FollowingDetailGroupByMajorDataSet
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public IEnumerable<TrainingProgramGroupByMajorDataSet> TrainingProgramGroupByMajorDataSets { get; set; }
    }

    public class TrainingProgramGroupByMajorDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<UniversityGroupByTrainingProgramDataSet> UniversityGroupByTrainingProgramDataSets { get; set; }
    }

    public class UniversityGroupByTrainingProgramDataSet
    {
        public int FollowingDetailId { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public string Phone { get; set; }
        public string WebUrl { get; set; }
        public int? TuitionType { get; set; }
        public int? TuitionFrom { get; set; }
        public int? TuitionTo { get; set; }
        public int? Rating { get; set; }
        public string MajorCode { get; set; }
        public SeasonDataSet SeasonDataSet { get; set; }
        public int? PositionOfUser { get; set; }
        public int TotalUserCared { get; set; }
        public int SubjectGroupId { get; set; }
        public string SubjectGroupCode { get; set; }
        public double? RankingMark { get; set; }
    }
}
