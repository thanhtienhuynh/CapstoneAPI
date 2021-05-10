using CapstoneAPI.DataSets.FCM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.FirebaseService
{
    public interface IFCMService
    {
        Task<FCMResponse> SendNotification(List<string> clientToken, string title, string body);
        Task<FCMResponse> SendToTopic(string topic, string title,
                                        string body, Dictionary<string, string> data);
    }
}
