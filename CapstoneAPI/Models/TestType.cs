using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class TestType
    {
        public TestType()
        {
            Tests = new HashSet<Test>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Test> Tests { get; set; }
    }
}
