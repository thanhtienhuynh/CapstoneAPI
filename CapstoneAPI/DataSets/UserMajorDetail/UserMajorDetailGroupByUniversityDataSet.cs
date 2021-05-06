using CapstoneAPI.DataSets.University;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.UserMajorDetail
{
    public class UserMajorDetailGroupByUniversityDataSet
    {
        public int UniversityId { get; set; }
        public CreateUniversityDataset University { get; set; }
        public IEnumerable<UserMajorDetailGroupByUniversityToReturn> DetailOfDataSets { get; set; }
    }
    public class UserMajorDetailGroupByUniversityToReturn
    {
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public string UniversityMajorCode { get; set; }
        public int TrainingProgramId { get; set; }
        public string TrainingProgramName { get; set; }
        public double? NewestEntryMark { get; set; }
        public int? NumberOfStudent { get; set; }
        public int YearOfEntryMark { get; set; }
        public int PositionOfUser { get; set; }
        public int TotalUserCared { get; set; }
        public int SubjectGroupId { get; set; }
        public string SubjectGroupCode { get; set; }
    }
}

