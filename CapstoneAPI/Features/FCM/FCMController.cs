using CapstoneAPI.Features.FCM.DataSet;
using CapstoneAPI.Features.FCM.Service;
using CapstoneAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.FCM
{
    [Route("api/v1/FirebaseCloudMessaging")]
    [ApiController]
    public class FCMController : ControllerBase
    {
        private readonly IFCMService _service;
        public FCMController(IFCMService service)
        {
            _service = service;
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost("send-to-topic")]
        public async Task<ActionResult<FCMResponse>> SendToTopic([FromQuery] TopicParam param)
        {
            var res = await _service.SendToTopic(param.Topic, param.Title, param.Body, param.Data);

            if (res == null)
                return NoContent();
            return Ok(res);
        }

        [Authorize(Roles = Roles.Staff)]
        [HttpPost("send-notification")]
        public async Task<ActionResult<FCMResponse>> SendNotification([FromQuery] NotificationParam param)
        {
            var res = await _service.SendNotification(param.ClientToken, param.Title, param.Body);

            if (res == null)
                return NoContent();
            return Ok(res);
        }
    }
}
