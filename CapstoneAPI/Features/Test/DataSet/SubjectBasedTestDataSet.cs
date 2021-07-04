using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Test.DataSet
{
    public class SubjectBasedTestDataSet
    {
        public int? SubjectId { get; set; }
        public int? UniversityId { get; set; }
        public List<TestDataSet> Tests { get; set; }
    }
}
