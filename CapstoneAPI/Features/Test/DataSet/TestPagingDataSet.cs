using CapstoneAPI.DataSets.Question;
using CapstoneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Test.DataSet
{
    public class TestPagingDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Level { get; set; }
        public int NumberOfQuestion { get; set; }
        public int? Year { get; set; }
        public int? SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int TestTypeId { get; set; }
        public string TestTypeName { get; set; }
        public int? TimeLimit { get; set; }
    }
}
