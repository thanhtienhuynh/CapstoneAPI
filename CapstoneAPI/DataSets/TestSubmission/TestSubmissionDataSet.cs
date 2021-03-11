using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.TestSubmission
{
    public class TestSubmissionDataSet
    {
        public int Id { get; set; }
        public int? TestId { get; set; }
        public int? UserId { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public int? SpentTime { get; set; }
        public double? Mark { get; set; }
        public int? NumberOfRightAnswers { get; set; }
        public int NumberOfQuestion { get; set; }

    }
}
