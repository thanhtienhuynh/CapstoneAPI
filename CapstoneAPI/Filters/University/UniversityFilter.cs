using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Filters.University
{
    public class UniversityFilter
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? TuitionType { get; set; }
        public int? Status { get; set; }
        /*
         * 0: InActive
         * 1: Active
         */
        public int Order { get; set; }
        /*
         * 0: Sort by DESC Code
         * 1: Sort by ASC Code
         * 2: Sort by DESC Name
         * 3: Sort by ASC Name
         * 4: Sort by DESC TuitionType
         * 5: Sort by ASC TuitionType
         */
    }
}
