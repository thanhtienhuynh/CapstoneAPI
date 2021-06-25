using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Filters.Test
{
    public class TestFilter
    {
        public string Name { get; set; }
        public int? Year { get; set; }
        public int? TestTypeId { get; set; }
        public int? SubjectId { get; set; }
        public int Order { get; set; }
        /*
         * 0: Sort by DESC Type
         * 1: Sort by ASC Type
         * 2: Sort by DESC Name
         * 3: Sort by ASC Name
         * 4: Sort by DESC Year
         * 5: Sort by ASC Year
         */
    }
}
