using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Career
    {
        public Career()
        {
            MajorCareers = new HashSet<MajorCareer>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }

        public virtual ICollection<MajorCareer> MajorCareers { get; set; }
    }
}
