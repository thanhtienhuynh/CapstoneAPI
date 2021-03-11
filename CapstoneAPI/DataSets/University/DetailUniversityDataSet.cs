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
        public string Infomation { get; set; }
        public int? Rating { get; set; }
        public int Status { get; set; }
        public string Tuition { get; set; }

        public List<UniMajorDataSet> Majors { get; set; }
    }

    public class UniMajorDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int? Status { get; set; }

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
}
