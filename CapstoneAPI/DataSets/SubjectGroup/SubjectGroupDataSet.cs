using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.SpecialSubjectGroup;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
}
