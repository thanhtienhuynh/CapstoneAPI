using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Features.Notification.DataSet;
using CapstoneAPI.Features.Notification.Service;
using CapstoneAPI.Filters;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Features.Notification
{
    [Route("api/v1/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }


        [HttpGet("user")]
        public async Task<ActionResult<PagedResponse<List<NotificationDataSet>>>> GetNotificationsByUser([FromQuery] PaginationFilter filter)
        {
            string token = Request.Headers["Authorization"];
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            return Ok(await _service.GetNotificationsByUser(token, validFilter));
        }

        [HttpGet("unread")]
        public async Task<ActionResult<Response<int>>> GetNumOfUnreadNoti()
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.GetNumberUnread(token));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Response<bool>>> MarkAsRead([FromRoute] int id)
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.MarkAsRead(token, id));
        }

        [HttpPut()]
        public async Task<ActionResult<Response<bool>>> MarkAsAllRead()
        {
            string token = Request.Headers["Authorization"];
            return Ok(await _service.MarkAsAllRead(token));
        }
    }
}
