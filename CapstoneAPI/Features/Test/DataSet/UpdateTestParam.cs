using CapstoneAPI.DataSets.Question;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Test.DataSet
{
    public class UpdateTestParam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Level { get; set; }
        public int Status { get; set; }
        public int? Year { get; set; }
        public int? SubjectId { get; set; }
        public int TestTypeId { get; set; }
        public int? UniversityId { get; set; }
        public int? TimeLimit { get; set; }
        public bool IsSuggestedTest { get; set; }
        public List<UpdateQuestionParam> Questions { get; set; }
    }
}
