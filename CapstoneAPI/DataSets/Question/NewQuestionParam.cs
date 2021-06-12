using CapstoneAPI.DataSets.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Question
{
    public class NewQuestionParam
    {
        public string QuestionContent { get; set; }
        public int Type { get; set; }
        public bool IsAnnotate { get; set; }
        public int? Ordinal { get; set; }
        public string Result { get; set; }
        public List<NewOptionParam> Options { get; set; }

    }
}
