﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.TestSubmission.DataSet
{
    public class SaveTestSubmissionParam
    {
        public int TestId { get; set; }
        public int? TestSubmissionId { get; set; }
        public int SpentTime { get; set; }
        public double Mark { get; set; }
        public int NumberOfRightAnswers { get; set; }

        public List<QuestionParam> Questions { get; set; }
    }

    public class FirstTestSubmissionParam
    {
        public int TestId { get; set; }
    }
}
