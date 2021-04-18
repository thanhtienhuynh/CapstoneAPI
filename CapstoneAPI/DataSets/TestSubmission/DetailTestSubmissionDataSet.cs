using CapstoneAPI.DataSets.QuestionSubmission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.TestSubmission
{
    public class DetailTestSubmissionDataSet
    {
        public int Id { get; set; }
        public int? TestId { get; set; }
        public string TestName { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public int? SpentTime { get; set; }
        public double? Mark { get; set; }
        public int? NumberOfRightAnswers { get; set; }
        public int NumberOfQuestion { get; set; }
        public int TimeLimit { get; set; }
        public int NumberOfCompletion { get; set; }
        public List<QuestionSubmissionDataSet> QuestionSubmissions { get; set; }
    }
}
