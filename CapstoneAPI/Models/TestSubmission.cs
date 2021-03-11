using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class TestSubmission
    {
        public TestSubmission()
        {
            QuestionSubmisstions = new HashSet<QuestionSubmisstion>();
        }

        public int Id { get; set; }
        public int? TestId { get; set; }
        public string UserId { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public int? SpentTime { get; set; }
        public double? Mark { get; set; }
        public int? NumberOfRightAnswers { get; set; }

        public virtual Test Test { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<QuestionSubmisstion> QuestionSubmisstions { get; set; }
    }
}
