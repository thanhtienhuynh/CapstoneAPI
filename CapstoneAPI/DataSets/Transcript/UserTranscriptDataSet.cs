using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Transcript
{
    public class UserTranscriptDataSet
    {
        public int TranscriptTypeId { get; set; }
        public string TranscriptTypeName { get; set; }
        public int Priority { get; set; }
        public List<UserTranscriptDetailDataSet> TranscriptDetails { get;set; }
    }

    public class UserTranscriptDetailDataSet
    {
        public int TransriptId { get; set; }
        public double Mark { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public DateTime DateRecord { get; set; }
    }
}
