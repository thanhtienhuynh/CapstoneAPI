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
        public double? DaysRemaining { get; set; }
        public double? LastTranscript { get; set; }
        public TestDataSet Test { get; set; }
    }
}
