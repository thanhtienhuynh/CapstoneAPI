using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Season.DataSet
{
    public class CreateSeasonParam
    {
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int SeasonSourceId { get; set; }
    }
}
