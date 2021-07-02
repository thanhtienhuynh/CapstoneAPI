﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.TestSubmission
{
    public class UserTestSubmissionDataSet
    {
        public int Id { get; set; }
        public int? TestId { get; set; }
        public string TestName { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int? SpentTime { get; set; }
        public double? Mark { get; set; }
        public int? NumberOfRightAnswers { get; set; }
        public int NumberOfQuestion { get; set; }
        public int TimeLimit { get; set; }
        public int NumberOfCompletion { get; set; }
    }

    public class UserTestSubmissionQueryParam
    {
        public int? SubjectId { get; set; }
        public int? TestTypeId { get; set; }
        public bool? IsSuggestedTest { get; set; }
        public int? Order { get; set; }
        //1: DateSubmission DESC
        //2: DateSubmission ASC
        //3: Year DESC
        //4: Year ASC
        //5: Name DESC
        //6: Name ASC
    }
}
