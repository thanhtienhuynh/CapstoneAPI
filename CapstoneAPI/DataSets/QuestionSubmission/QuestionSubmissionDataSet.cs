using CapstoneAPI.DataSets.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.QuestionSubmission
{
    public class QuestionSubmissionDataSet
    {
        public int Id { get; set; }
        public string Result { get; set; }
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public int NumberOfOption { get; set; }
        public string RightResult { get; set; }
        public int RealOrder { get; set; }
        public bool IsAnnotate { get; set; }
        public int Type { get; set; }
        public int TestId { get; set; }
        public List<OptionDataSet> Options { get; set; }
    }
}
