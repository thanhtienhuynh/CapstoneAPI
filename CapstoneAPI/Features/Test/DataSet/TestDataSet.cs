using CapstoneAPI.DataSets.Question;
using CapstoneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Test.DataSet
{
    public class TestDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Level { get; set; }
        public int NumberOfQuestion { get; set; }
        public int? Year { get; set; }
        public DateTime CreateDate { get; set; }
        public int? SubjectId { get; set; }
        public int TestTypeId { get; set; }
        public int? UniversityId { get; set; }
        public int? TimeLimit { get; set; }
        public List<QuestionDataSet> Questions { get; set; }
    }
}
