using AutoMapper;
using CapstoneAPI.Features.MajorSubjectGroup.DataSet;
using CapstoneAPI.Features.Notification.DataSet;
using CapstoneAPI.Filters;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Notification.Service
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<NotificationService>();
        private IMapper _mapper;

        public NotificationService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
    }

        public async Task<PagedResponse<List<NotificationDataSet>>> GetNotificationsByUser(string token, PaginationFilter validFilter)
        {
            PagedResponse<List<NotificationDataSet>> response = new PagedResponse<List<NotificationDataSet>>();
            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }
                IEnumerable<Models.Notification> allNotifications = await _uow.NotificationRepository
                        .Get(filter: m => m.UserId == user.Id, orderBy: n => n.OrderByDescending(n => n.DateRecord));
                List<NotificationDataSet> result = allNotifications.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                                                        .Take(validFilter.PageSize)
                                                        .Select(n => _mapper.Map<NotificationDataSet>(n)).ToList();
                foreach (NotificationDataSet notification in result)
                {
                    notification.TimeAgo = JWTUtils.CalculateTimeAgo(notification.DateRecord);
                }
                response = PaginationHelper.CreatePagedReponse(result, validFilter, allNotifications.Count());
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<int>> GetNumberUnread(string token)
        {
            Response<int> response = new Response<int>();
            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }
                int count = await _uow.NotificationRepository.Count(
                    filter: n => n.UserId == user.Id && !n.IsRead);

                response.Data = count;
                response.Succeeded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<bool>> MarkAsRead(string token, int notiId)
        {
            Response<bool> response = new Response<bool>();
            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }
                Models.Notification notification = await _uow.NotificationRepository
                        .GetById(notiId);

                if (notification != null)
                {
                    notification.IsRead = true;
                    _uow.NotificationRepository.Update(notification);
                    await _uow.CommitAsync();
                }
                response.Data = true;
                response.Succeeded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<bool>> MarkAsAllRead(string token)
        {
            Response<bool> response = new Response<bool>();
            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }
                IEnumerable<Models.Notification> allUnreadNotifications = await _uow.NotificationRepository
                        .Get(filter: m => m.UserId == user.Id && !m.IsRead);

                foreach (Models.Notification notification in allUnreadNotifications)
                {
                    notification.IsRead = true;
                }
                _uow.NotificationRepository.UpdateRange(allUnreadNotifications);
                await _uow.CommitAsync();
                response.Data = true;
                response.Succeeded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }
        
    }
}
