﻿using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Rank
    {
        public int UserMajorDetailId { get; set; }
        public int RankTypeId { get; set; }
        public int Position { get; set; }
        public double TotalMark { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsReceiveNotification { get; set; }
        public bool IsNew { get; set; }

        public virtual RankType RankType { get; set; }
        public virtual UserMajorDetail UserMajorDetail { get; set; }
    }
}