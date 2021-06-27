using CapstoneAPI.DataSets.FCM;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.FirebaseService
{
    public class FCMService : IFCMService
    {
        public async Task<FCMResponse> SendNotification(List<string> clientToken, string title, string body)
        {
            var message = new MulticastMessage
            {
                Tokens = clientToken,
                Data = new Dictionary<string, string>()
                {
                     {"title" , title},
                     {"body" , body }
                },
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };
            var response = await FirebaseMessaging.DefaultInstance
                                .SendMulticastAsync(message).ConfigureAwait(true);

            var res = new FCMResponse
            {
                Message = response.Responses[0]?.Exception?.Message,
                SuccessCount = response.SuccessCount,
                FailedCount = response.FailureCount
            };

            return res;
        }

        public async Task<FCMResponse> SendToTopic(string topic, string title, string body, Dictionary<string, string> data)
        {
            var message = new Message
            {
                Topic = topic,
                Data = data,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            var response = await FirebaseMessaging.DefaultInstance
                                .SendAsync(message).ConfigureAwait(true);

            var res = new FCMResponse
            {
                Message = response
            };

            return res;
        }

        public async Task<BatchResponse> SendBatchMessage(List<Message> messages)
        {

            var response = await FirebaseMessaging.DefaultInstance
                .SendAllAsync(messages).ConfigureAwait(true);

            var res = new FCMResponse
            {
                Message = response.SuccessCount.ToString()
            };

            return response;
        }
    }
}
