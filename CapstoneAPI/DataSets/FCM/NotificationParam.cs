using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.FCM
{
    public class NotificationParam
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public List<string> ClientToken { get; set; }

    }
}
