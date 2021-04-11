using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.University
{
    public class DetailUniversityDataSet
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
        public int Status { get; set; }

        public List<UniMajorDataSet> Majors { get; set; }
    }

    public class UniMajorDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? Status { get; set; }
        public int? NumberOfStudents { get; set; }
        public int TrainingProgramId { get; set; }
        public string TrainingProgramName { get; set; }
        public List<UniSubjectGroupDataSet> SubjectGroups { get; set; }
    }

    public class UniSubjectGroupDataSet
    {
        public int Id { get; set; }
        public string GroupCode { get; set; }
        public int Status { get; set; }
        public List<UniEntryMarkDataSet> EntryMarks { get; set; }
    }

    public class UniEntryMarkDataSet
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public double Mark { get; set; }
    }

    public class AddingMajorUniversityParam
    {
        public int UniversityId { get; set; }
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public string MajorCode { get; set; }
        public int NumberOfStudents { get; set; }
        public int TrainingProgramId { get; set; }
        public List<UniSubjectGroupDataSet> SubjectGroups { get; set; }
    }

    public class UpdatingMajorUniversityParam
    {
        public int UniversityId { get; set; }
        public int MajorId { get; set; }
        public string MajorCode { get; set; }
        public int NumberOfStudents { get; set; }
        public int OldTrainingProgramId { get; set; }
        public int NewTrainingProgramId { get; set; }
        public List<UpdatingUniSubjectGroupDataSet> SubjectGroups { get; set; }
    }

    public class UpdatingUniSubjectGroupDataSet
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public List<UniEntryMarkDataSet> EntryMarks { get; set; }
    }
}
