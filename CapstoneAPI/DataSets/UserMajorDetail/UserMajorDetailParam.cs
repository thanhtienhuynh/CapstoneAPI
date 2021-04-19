using CapstoneAPI.DataSets.SubjectGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.UserMajorDetail
{
    public class AddUserMajorDetailParam
    {
        public int UniversityId { get; set; }
        public int TrainingProgramId { get; set; }
        public int MajorId { get; set; }
        public SubjectGroupParam SubjectGroupParam { get; set; }

    }

    public class UpdateUserMajorDetailParam
    {
        public int UniversityId { get; set; }
        public int TrainingProgramId { get; set; }
        public int MajorId { get; set; }
    }
}
