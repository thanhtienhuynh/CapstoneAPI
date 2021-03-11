using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Option
    {
        public int Id { get; set; }
        public string OptionContent { get; set; }
        public int QuestionId { get; set; }
        public int? Ordinal { get; set; }

        public virtual Question Question { get; set; }
    }
}
