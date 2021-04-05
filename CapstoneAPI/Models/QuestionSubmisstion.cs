using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class QuestionSubmisstion
    {
        public int Id { get; set; }
        public string Result { get; set; }
        public int QuestionId { get; set; }
        public int TestSubmissionId { get; set; }
        public virtual Question Question { get; set; }
        public virtual TestSubmission TestSubmission { get; set; }
    }
}
