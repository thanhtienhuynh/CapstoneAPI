using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.TestSubmission
{
    public class TestSubmissionParam
    {
        public int? UserId { get; set; }
        public int? TestId { get; set; }
        public int? SpentTime { get; set; }
        public List<QuestionParam> Questions { get; set; }
    }

    public class QuestionParam
    {
        public int Id { get; set; }
        public string Options { get; set; }
    }
}
