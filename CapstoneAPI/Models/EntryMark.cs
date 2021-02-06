using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class EntryMark
    {
        public int Id { get; set; }
        public int WeightNumberId { get; set; }
        public int UniversityId { get; set; }
        public DateTime Year { get; set; }
        public int Mark { get; set; }

        public virtual University University { get; set; }
        public virtual WeightNumber WeightNumber { get; set; }
    }
}
