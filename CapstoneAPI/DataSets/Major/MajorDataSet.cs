using CapstoneAPI.DataSets.Article;
using CapstoneAPI.DataSets.University;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Major
{
    public class MajorDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double WeightMark { get; set; }
        public double HighestEntryMark { get; set; }
    }

    public class MajorDetailDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Curriculum { get; set; }
        public string HumanQuality { get; set; }
        public string SalaryDescription { get; set; }
        public List<DetailUniversityDataSet> Universities { get; set; }
        public List<ArticleDetailDataSet> Articles { get; set; }
        public List<CareerDataSet> Careers { get; set; }
        public List<MajorDetailSubjectGroupDataSet> SubjectGroups { get; set; }
    }

    public class CareerDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class MajorDetailSubjectGroupDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> Subjects { get; set; }
    }
}
