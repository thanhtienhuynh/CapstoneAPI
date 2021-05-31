using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class RankType
    {
        public RankType()
        {
            Ranks = new HashSet<Rank>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }

        public virtual ICollection<Rank> Ranks { get; set; }
    }
}
