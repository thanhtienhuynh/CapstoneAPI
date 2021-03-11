using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Question
    {
        public Question()
        {
            Options = new HashSet<Option>();
            QuestionSubmisstions = new HashSet<QuestionSubmisstion>();
        }

        public int Id { get; set; }
        public string QuestionContent { get; set; }
        public int NumberOfOption { get; set; }
        public string Result { get; set; }
        public int Type { get; set; }
        public int TestId { get; set; }
        public bool IsAnnotate { get; set; }
        public int? Ordinal { get; set; }

        public virtual Test Test { get; set; }
        public virtual ICollection<Option> Options { get; set; }
        public virtual ICollection<QuestionSubmisstion> QuestionSubmisstions { get; set; }
    }
}
