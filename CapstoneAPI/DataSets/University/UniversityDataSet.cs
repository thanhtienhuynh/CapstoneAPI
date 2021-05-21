using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.University
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
        public List<TrainingProgramDataSet> TraingProgramSets { get; set; }
    }

    public class TrainingProgramDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? NumberOfCaring { get; set; }
        public bool IsCared { get; set; }
        public int Rank { get; set; }
        public List<SeasonDataSet> SeasonDataSets { get; set; }
    }

    public class SeasonDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? NumberOfStudents { get; set; }
        public double? EntryMark { get; set; }
    }
}
