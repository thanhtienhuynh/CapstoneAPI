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
        public int? SeasonSourceId { get; set; }
    }

    public class UpdateSeasonParam
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public int Status { get; set; }
    }
}
