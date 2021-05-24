using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class FollowingDetail
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int EntryMarkId { get; set; }
        public bool IsReceiveNotification { get; set; }

        public virtual EntryMark EntryMark { get; set; }
        public virtual User User { get; set; }
        public virtual Rank Rank { get; set; }
    }
}
