using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.FCM
{
    public class TopicParam
    {
        public string Topic { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}
