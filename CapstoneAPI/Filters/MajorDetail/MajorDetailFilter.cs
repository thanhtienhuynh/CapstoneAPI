using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Filters.MajorDetail
{
    public class MajorDetailFilter
    {
        public int UniversityId { get; set; }
        public int SeasonId { get; set; }
        public string MajorCode { get; set; }
        public string MajorName { get; set; }
        public int Order { get; set; }
        /*
         * 0: Sort by DESC Code
         * 1: Sort by DESC Code
         * 2: Sort by ASC Code
         * 3: Sort by DESC Name
         * 4: Sort by ASC Name
         */
    }
}
