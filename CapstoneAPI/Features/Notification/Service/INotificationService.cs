using CapstoneAPI.Features.MajorSubjectGroup.DataSet;
using CapstoneAPI.Features.Notification.DataSet;
using CapstoneAPI.Filters;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Notification.Service
{
    public interface INotificationService
    {
        Task<PagedResponse<List<NotificationDataSet>>> GetNotificationsByUser(string token, PaginationFilter validFilter);
        Task<Response<int>> GetNumberUnread(string token);
        Task<Response<bool>> MarkAsRead(string token, int notiId);
        Task<Response<bool>> MarkAsAllRead(string token);

    }
}
