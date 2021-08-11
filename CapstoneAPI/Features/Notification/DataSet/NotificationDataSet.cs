using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Notification.DataSet
{
    public class NotificationDataSet
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime DateRecord { get; set; }
        public bool IsRead { get; set; }
        public string Data { get; set; }
        public int Type { get; set; }
        public string TimeAgo { get; set; }
    }
}
