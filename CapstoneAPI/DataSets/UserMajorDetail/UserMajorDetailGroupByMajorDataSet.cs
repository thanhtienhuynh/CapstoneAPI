using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.UserMajorDetail
{
    public class UserMajorDetailGroupByMajorDataSet
    {
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public IEnumerable<UserMajorDetailToReturn> DetailOfDataSets { get; set; }
    }
    public class UserMajorDetailToReturn
    {
        public int UniversityId { get; set; }
        public string UniversityName { get; set; }
        public string UniversityLogo { get; set; }
        public string UniversityDescription { get; set; }
        public string UniversityCode { get; set; }
        public string UniversityAddress { get; set; }
        public string UniversityPhone { get; set; }
        public string UniversityWebUrl { get; set; }
        public int? TuitionType { get; set; }
        public int? TuitionFrom { get; set; }
        public int? TuitionTo { get; set; }
        public int? Rating { get; set; }

        public string UniversityMajorCode { get; set; }
        public double? NewestEntryMark { get; set; }
        public int? NumberOfStudent { get; set; }
        public int YearOfEntryMark { get; set; }
        public int PositionOfUser { get; set; }
        public int TotalUserCared { get; set; }
        public int SubjectGroupId { get; set; }
        public string SubjectGroupCode { get; set; }
    }
}
