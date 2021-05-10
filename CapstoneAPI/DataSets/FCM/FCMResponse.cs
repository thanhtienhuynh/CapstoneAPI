using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.DataSets.FCM
{
    public class FCMResponse
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; }
    }
}
