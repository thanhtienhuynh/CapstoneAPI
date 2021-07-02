using CapstoneAPI.DataSets.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Question
{
    public class UpdateQuestionParam
    {
        public string Content { get; set; }
        public int Type { get; set; }
        public bool IsAnnotate { get; set; }
        public int Ordinal { get; set; }
        public List<UpdateOptionParam> Options { get; set; }
    }
}
