using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.Major
{
    public class MajorDataSet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double WeightMark { get; set; }
        public int TrainingProgramId { get; set; }
        public string TrainingProgramName { get; set; }

        public double HighestEntryMark { get; set; }
    }
}
