using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Test
    {
        public Test()
        {
            Questions = new HashSet<Question>();
            TestSubmissions = new HashSet<TestSubmission>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? Level { get; set; }
        public int NumberOfQuestion { get; set; }
        public int Status { get; set; }
        public DateTime? Year { get; set; }
        public DateTime CreateDate { get; set; }
        public int? SubjectId { get; set; }
        public int UserId { get; set; }
        public int TestTypeId { get; set; }
        public int? UniversityId { get; set; }
        public int? TimeLimit { get; set; }

        public virtual Subject Subject { get; set; }
        public virtual TestType TestType { get; set; }
        public virtual University University { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<TestSubmission> TestSubmissions { get; set; }
    }
}
