using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.TestSubmission
{
    public class SaveTestSubmissionParam
    {
        public int? TestId { get; set; }
        public int? SpentTime { get; set; }
        public double Mark { get; set; }
        public int NumberOfRightAnswers { get; set; }
        public int MajorId { get; set; }
        public int UniversityId { get; set; }

        public List<QuestionParam> Questions { get; set; }
    }
}
