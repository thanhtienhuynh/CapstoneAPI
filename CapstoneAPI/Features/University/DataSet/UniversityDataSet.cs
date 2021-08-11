using CapstoneAPI.Features.FollowingDetail.DataSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.University.DataSet
{
    public class TrainingProgramBasedUniversityDataSet
    {
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
        public double HighestEntryMark { get; set; }
        public List<TrainingProgramDataSet> TrainingProgramSets { get; set; }
    }

    public class TrainingProgramDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfCaring { get; set; }
        public FollowingDetailDataSet FollowingDetail { get; set; }
        public int Rank { get; set; }
        public double? Ratio { get; set; }
        public int DividedClass { get; set; }
        public List<SeasonDataSet> SeasonDataSets { get; set; }
    }

    public class SeasonDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? NumberOfStudents { get; set; }
        public double? EntryMark { get; set; }
    }

    public class MockTestBasedUniversity
    {
        public int MajorId { get; set; }
        public int SubjectGroupId { get; set; }
        public double TotalMark { get; set; }
        public List<TrainingProgramBasedUniversityDataSet> TrainingProgramBasedUniversityDataSets { get; set; }
    }
}
