using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.TestSubmission.DataSet
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
        public int SubjectId { get; set; }
        public List<ResultQuestion> ResultQuestions { get; set; }
    }

    public class ResultQuestion
    {
        public int Id { get; set; }
        public string Result { get; set; }
    }
}
