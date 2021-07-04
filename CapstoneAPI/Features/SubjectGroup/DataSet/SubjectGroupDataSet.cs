using CapstoneAPI.DataSets.SpecialSubjectGroup;
using CapstoneAPI.Features.Major.DataSet;
using CapstoneAPI.Features.Subject.DataSet;
using CapstoneAPI.Features.Transcript.DataSet;
using System.Collections.Generic;

namespace CapstoneAPI.Features.SubjectGroup.DataSet
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
