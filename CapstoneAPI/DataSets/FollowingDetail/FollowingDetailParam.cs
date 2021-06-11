using CapstoneAPI.DataSets.Rank;
using CapstoneAPI.DataSets.SubjectGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.FollowingDetail
{
    public class AddFollowingDetailParam
    {
        public int UniversityId { get; set; }
        public int TrainingProgramId { get; set; }
        public int MajorId { get; set; }
        public int SubjectGroupId { get; set; }
        public double TotalMark { get; set; }
        public SubjectGroupParam SubjectGroupParam { get; set; }

    }

    public class FollowingDetailDataSet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EntryMarkId { get; set; }
        public bool IsReceiveNotification { get; set; }
    }
}
