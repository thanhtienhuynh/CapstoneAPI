using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.FCM.DataSet
{
    public class NotificationParam
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public List<string> ClientToken { get; set; }

    }
}
