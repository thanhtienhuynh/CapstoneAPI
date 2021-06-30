using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Filters.Major
{
    public class UniversityToMajorFilter
    {
        public int Id { get; set; }
        public string UniversityCode { get; set; }
        public string UniversityName { get; set; }
        public int SeasonId { get; set; }
        public int Order { get; set; }
        /*
         * 0: Sort by DESC Code
         * 1: Sort by ASC Code
         * 2: Sort by DESC Name
         * 3: Sort by ASC Name
         */
    }
}
