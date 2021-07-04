using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Transcript.DataSet
{
    public class UserTranscriptTypeDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
