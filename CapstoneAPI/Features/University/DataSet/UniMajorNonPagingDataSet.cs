using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.University.DataSet
{

    public class UniMajorNonPagingDataSet
    {
        public int UniversityId { get; set; }
        public int MajorId { get; set; }
        public string MajorCode { get; set; }
        public string MajorName { get; set; }
        public List<MajorDetailUniNonPagingDataSet> MajorDetailUnies { get; set; }
    }

    public class MajorDetailUniNonPagingDataSet
    {
        public int Id { get; set; }
        public int TrainingProgramId { get; set; }
        public string TrainingProgramName { get; set; }
        public int? AdmissionQuantity { get; set; }
        public string MajorDetailCode { get; set; }
        public int SeasonId { get; set; }
        public string SeasonName { get; set; }
    }
}

