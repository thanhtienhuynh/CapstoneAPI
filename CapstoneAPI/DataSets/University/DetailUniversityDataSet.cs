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
        public int UniversityId { get; set; }
        public int MajorId { get; set; }
        public string MajorCode { get; set; }
        public string MajorName { get; set; }
        public List<MajorDetailUniDataSet> MajorDetailUnies { get; set; }
    }

    public class MajorDetailUniDataSet
    {
        public int TrainingProgramId { get; set; }
        public string TrainingProgramName { get; set; }
        public int? AdmissionQuantity { get; set; }
        public string MajorDetailCode { get; set; }
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
        public List<MajorDetailSubAdmissionDataSet> MajorDetailSubAdmissions { get; set; }
    }
    public class MajorDetailSubAdmissionDataSet
    {
        public int Id { get; set; }
        public int? Quantity { get; set; }
        public int? GenderId { get; set; }
        public int AdmissionMethodId { get; set; }
        public string AdmissionMethodName { get; set; }
        public int? ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public List<MajorDetailEntryMarkDataset> MajorDetailEntryMarks { get; set; }
    }
    public class MajorDetailEntryMarkDataset
    {
        public int Id { get; set; }
        public double? Mark { get; set; }
        public int MajorSubjectGoupId { get; set; }
        public int SubjectGroupId { get; set; }
        public string SubjectGroupCode { get; set; }
    }






    public class UniMajorDetailDataSet
    {
        public int Id { get; set; }

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
        public string MajorCode { get; set; }
        public int SeasonId { get; set; }
        public int TrainingProgramId { get; set; }
        public int? TotalAdmissionQuantity { get; set; }
        public List<UniSubAdmissionDataSet> SubAdmissions { get; set; }
    }
    public class UniSubjectGroupDataSet
    {
        public int Id { get; set; }
        public double? EntryMarkPerGroup { get; set; }
    }
    public class UniSubAdmissionDataSet
    {
        public int? Quantity { get; set; }
        public int? GenderId { get; set; }
        public int AdmissionMethodId { get; set; }
        public int? ProvinceId { get; set; }
        public List<UniSubjectGroupDataSet> SubjectGroups { get; set; }

    }


    public class UpdatingMajorUniversityParam
    {
        public int UniversityId { get; set; }
        public int MajorId { get; set; }
        public string MajorCode { get; set; }
        public int? NumberOfStudents { get; set; }
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
