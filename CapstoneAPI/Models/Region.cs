using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Region
    {
        public Region()
        {
            Provinces = new HashSet<Province>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Province> Provinces { get; set; }
    }
}
