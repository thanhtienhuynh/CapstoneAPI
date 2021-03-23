using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class User
    {
        public User()
        {
            TestSubmissions = new HashSet<TestSubmission>();
            Tests = new HashSet<Test>();
            Transcripts = new HashSet<Transcript>();
            UserMajors = new HashSet<UserMajor>();
            UserUniversities = new HashSet<UserUniversity>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<TestSubmission> TestSubmissions { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
        public virtual ICollection<Transcript> Transcripts { get; set; }
        public virtual ICollection<UserMajor> UserMajors { get; set; }
        public virtual ICollection<UserUniversity> UserUniversities { get; set; }
    }
}
