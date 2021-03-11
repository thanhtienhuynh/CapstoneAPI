using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Test
{
    public class SubjectBasedTestDataSet
    {
        public int? SubjectId { get; set; }
        public int? UniversityId { get; set; }
        public List<TestDataSet> Tests { get; set; }
    }
}
