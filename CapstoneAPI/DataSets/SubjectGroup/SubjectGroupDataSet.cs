using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.SpecialSubjectGroup;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.Transcript;
using System.Collections.Generic;

namespace CapstoneAPI.DataSets.SubjectGroup
{
    public class SubjectGroupDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double TotalMark { get; set; }
        public List<SpecialSubjectGroupDataSet> SpecialSubjectGroupDataSets { get; set; }
        public List<SubjectDataSet> SubjectDataSets { get; set; }
        public List<MajorDataSet> SuggestedMajors { get; set; }
    }

    public class UserSuggestionInformation
    {
        public int TranscriptTypeId { get; set; }
        public string TranscriptTypeName { get; set; }
        public int? ProvinceId { get; set; }
        public int? Gender { get; set; }
        public IEnumerable<SubjectGroupDataSet> SubjectGroupDataSets { get; set; }
        public IEnumerable<UserTranscriptTypeDataSet> TranscriptDetails { get; set; }
    }
}
