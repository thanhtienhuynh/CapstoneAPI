using AutoMapper;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Article;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using FirebaseAdmin.Messaging;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CapstoneAPI.Features.Article.DataSet;
using CapstoneAPI.Features.FCM.Service;

namespace CapstoneAPI.Features.Article.Service
{
    public class ArticleService : IArticleService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IFCMService _firebaseService;
        private readonly ILogger _log = Log.ForContext<ArticleService>();

        public ArticleService(IUnitOfWork uow, IMapper mapper, IFCMService firebaseService)
        {
            _uow = uow;
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        public async Task<PagedResponse<List<ArticleCollapseDataSet>>> GetListArticleForGuest(PaginationFilter validFilter, string title)
        {
            PagedResponse<List<ArticleCollapseDataSet>> result = new PagedResponse<List<ArticleCollapseDataSet>>();

            try
            {

                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();

                Expression<Func<Models.Article, bool>> filter = null;
                filter = a => a.Status == Articles.Published && a.PublicFromDate != null && a.PublicToDate != null
                    && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0
                    && a.PublicFromDate <= currentDate
                    && (string.IsNullOrEmpty(title) || a.Title.Contains(title));

                IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                    .Get(filter: filter, orderBy: o => o.OrderByDescending(a => a.PostedDate),
                    first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);

                if (articles.Count() == 0)
                {
                    result.Succeeded = true;
                    result.Message = "Không có tin tức nào để hiển thị!";
                }
                else
                {
                    var articleCollapseDataSet = articles.Select(m => _mapper.Map<ArticleCollapseDataSet>(m)).ToList();
                    foreach(var article in articleCollapseDataSet)
                    {
                        article.TimeAgo = JWTUtils.CalculateTimeAgo(article.PublicFromDate);
                    }
                    var totalRecords = await _uow.ArticleRepository
                        .Count(filter: filter);
                    result = PaginationHelper.CreatePagedReponse(articleCollapseDataSet, validFilter, totalRecords);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return result;
        }

        public async Task<PagedResponse<List<AdminArticleCollapseDataSet>>> GetListArticleForAdmin(PaginationFilter validFilter,
            AdminArticleFilter articleFilter)
        {
            PagedResponse<List<AdminArticleCollapseDataSet>> result = new PagedResponse<List<AdminArticleCollapseDataSet>>();

            try
            {
                Expression<Func<Models.Article, bool>> filter = null;

                filter = a => (string.IsNullOrEmpty(articleFilter.Search) || a.Title.Contains(articleFilter.Search))
                && (articleFilter.PublicFromDate == null || articleFilter.PublicFromDate == DateTime.MinValue
                || a.PublicFromDate >= articleFilter.PublicFromDate)
                && (articleFilter.PublicToDate == null || articleFilter.PublicToDate == DateTime.MinValue
                || a.PublicToDate <= articleFilter.PublicToDate)
                && (articleFilter.PostedDate == null || articleFilter.PostedDate == DateTime.MinValue
                || a.PostedDate.Value.Date == articleFilter.PostedDate.Date)
                && (articleFilter.ImportantLevel == null || a.ImportantLevel == articleFilter.ImportantLevel)
                && (string.IsNullOrEmpty(articleFilter.PublishedPage) || a.PublishedPage.Equals(articleFilter.PublishedPage))
                && (articleFilter.Status < 0 || a.Status == articleFilter.Status);

                Func<IQueryable<Models.Article>, IOrderedQueryable<Models.Article>> order = null;
                switch (articleFilter.Order ?? 0)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.CrawlerDate);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.CrawlerDate);
                        break;
                    case 2:
                        order = order => order.OrderBy(a => a.Title);
                        break;
                    case 3:
                        order = order => order.OrderByDescending(a => a.Title);
                        break;
                    case 4:
                        order = order => order.OrderBy(a => a.PostedDate);
                        break;
                    case 5:
                        order = order => order.OrderByDescending(a => a.PostedDate);
                        break;
                    case 6:
                        order = order => order.OrderBy(a => a.ImportantLevel);
                        break;
                    case 7:
                        order = order => order.OrderByDescending(a => a.ImportantLevel);
                        break;
                }


                IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                    .Get(filter: filter, orderBy: order,
                    first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);

                if (articles.Count() == 0)
                {
                    result.Succeeded = true;
                    result.Message = "Không có tin tức nào để hiển thị!";
                }
                else
                {
                    var articleCollapseDataSet = articles.Select(m => _mapper.Map<AdminArticleCollapseDataSet>(m)).ToList();
                    var totalRecords = await _uow.ArticleRepository.Count(filter);
                    result = PaginationHelper.CreatePagedReponse(articleCollapseDataSet, validFilter, totalRecords);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }



            return result;
        }
        public async Task<Response<ArticleDetailDataSet>> GetArticleById(int id)
        {
            Response<ArticleDetailDataSet> result = new Response<ArticleDetailDataSet>();
            try
            {
                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();

                Models.Article article = await _uow.ArticleRepository.GetFirst(filter: a => a.Id == id &&
                    a.Status == Articles.Published && a.PublicFromDate != null && a.PublicToDate != null
                    && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0);
                if (article == null)
                {
                    if (result.Errors == null)
                        result.Errors = new List<string>();
                    result.Errors.Add("Không thể xem tin tức này!");
                }
                else
                {
                    var data = _mapper.Map<ArticleDetailDataSet>(article);
                    result = new Response<ArticleDetailDataSet>(data);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return result;
        }

        public async Task<Response<AdminArticleDetailDataSet>> AdminGetArticleById(int id)
        {
            Response<AdminArticleDetailDataSet> result = new Response<AdminArticleDetailDataSet>(); ;

            try
            {
                Models.Article article = await _uow.ArticleRepository.GetFirst(filter: a => a.Id == id,
                   includeProperties: "UniversityArticles,UniversityArticles.University,MajorArticles,MajorArticles.Article");

                if (article == null)
                {
                    if (result.Errors == null)
                        result.Errors = new List<string>();
                    result.Errors.Add("Không thể xem tin tức này!");
                }
                else
                {
                    var data = _mapper.Map<AdminArticleDetailDataSet>(article);

                    if (article.UniversityArticles != null)
                    {
                        if (data.UniversityIds == null)
                            data.UniversityIds = new List<int>();
                        foreach (var item in article.UniversityArticles)
                        {
                            data.UniversityIds?.Add(item.UniversityId);
                        }
                    }

                    if (article.MajorArticles != null)
                    {
                        if (data.MajorIds == null)
                            data.MajorIds = new List<int>();
                        foreach (var item in article.MajorArticles)
                        {
                            data.MajorIds?.Add(item.MajorId);
                        }
                    }

                    result = new Response<AdminArticleDetailDataSet>(data);
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return result;
        }

        public async Task<Response<ApprovingArticleDataSet>> UpdateStatusArticle(ApprovingArticleDataSet approvingArticleDataSet, string token)
        {
            Response<ApprovingArticleDataSet> response = new Response<ApprovingArticleDataSet>();
            try
            {
                int? oldStatus = null;
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

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                    return response;
                }
                if (approvingArticleDataSet != null)
                {
                    Models.Article articleToUpdate = await _uow.ArticleRepository
                        .GetFirst(filter: a => a.Id.Equals(approvingArticleDataSet.Id),
                        includeProperties: "UniversityArticles,UniversityArticles.University,MajorArticles,MajorArticles.Major");
                    if (articleToUpdate == null)
                    {
                        if (response.Errors == null)
                            response.Errors = new List<string>();
                        response.Errors.Add("Không thể tìm thấy tin tức để cập nhật!");
                        return response;
                    }
                    else
                    {
                        //check status
                        /*
                         * -1: All
                         * 0: New
                         * 1: Approved
                         * 2: Rejected
                         * 3: Published
                         * 4: Expired
                         * 5: (Considered)
                         */
                        if (articleToUpdate.Status != approvingArticleDataSet.Status)
                        {
                            if (!((articleToUpdate.Status == Articles.New && approvingArticleDataSet.Status == Articles.Approved)
                                || (articleToUpdate.Status == Articles.Rejected && approvingArticleDataSet.Status == Articles.New)
                                || (articleToUpdate.Status == Articles.New && approvingArticleDataSet.Status == Articles.Rejected)
                                || (articleToUpdate.Status == Articles.Approved && approvingArticleDataSet.Status == Articles.Rejected)
                                || (articleToUpdate.Status == Articles.Approved && approvingArticleDataSet.Status == Articles.Published)
                                || (articleToUpdate.Status == Articles.Published && approvingArticleDataSet.Status == Articles.Considered)
                                || (articleToUpdate.Status == Articles.Considered && approvingArticleDataSet.Status == Articles.Approved)
                                || (articleToUpdate.Status == Articles.Considered && approvingArticleDataSet.Status == Articles.Rejected)
                                || (articleToUpdate.Status == Articles.Expired && approvingArticleDataSet.Status == Articles.Published)
                                || (articleToUpdate.Status == Articles.Published && approvingArticleDataSet.Status == Articles.Expired)))
                            {
                                if (response.Errors == null)
                                    response.Errors = new List<string>();
                                response.Errors.Add("Trạng thái bài viết cập nhật không thành công!");
                                response.Succeeded = false;
                                return response;
                            }
                        }
                        //FIND USER TO SEND NOTI
                        oldStatus = articleToUpdate.Status;

                        if (approvingArticleDataSet.Status == Articles.Published)
                        {
                            if (approvingArticleDataSet.PublicFromDate == null || approvingArticleDataSet.PublicToDate == null)
                            {
                                if (response.Errors == null)
                                    response.Errors = new List<string>();
                                response.Errors.Add("Ngày đăng không được để trống!");
                                response.Succeeded = false;
                                return response;
                            } else
                            {
                                if (DateTime.Compare((DateTime)approvingArticleDataSet.PublicFromDate,
                                    (DateTime)approvingArticleDataSet.PublicToDate) >= 0
                                    || DateTime.Compare((DateTime)approvingArticleDataSet.PublicToDate,
                                    JWTUtils.GetCurrentTimeInVN()) < 0)
                                {
                                    if (response.Errors == null)
                                        response.Errors = new List<string>();
                                    response.Errors.Add("Ngày đăng không hợp lệ!");
                                    response.Succeeded = false;
                                    return response;
                                }
                            }
                        }

                        articleToUpdate.PublicFromDate = approvingArticleDataSet.PublicFromDate;
                        articleToUpdate.PublicToDate = approvingArticleDataSet.PublicToDate;
                        articleToUpdate.Status = approvingArticleDataSet.Status;
                        articleToUpdate.Censor = user.Id;

                        _uow.UniversityArticleRepository.DeleteComposite(filter: uniArt => uniArt.ArticleId == approvingArticleDataSet.Id);
                        foreach (var item in approvingArticleDataSet.University)
                        {
                            Models.University university = await _uow.UniversityRepository.GetById(item);
                            if (university == null)
                            {
                                continue;
                            }
                            Models.UniversityArticle universityArticle = new Models.UniversityArticle()
                            {
                                UniversityId = item,
                                ArticleId = approvingArticleDataSet.Id
                            };
                            _uow.UniversityArticleRepository.Insert(universityArticle);
                        }

                        _uow.MajorArticleRepository.DeleteComposite(filter: majorArt => majorArt.ArticleId == approvingArticleDataSet.Id);

                        foreach (var item in approvingArticleDataSet.Major)
                        {
                            Models.Major major = await _uow.MajorRepository.GetById(item);
                            if (major == null)
                            {
                                continue;
                            }
                            Models.MajorArticle majorArticle = new Models.MajorArticle()
                            {
                                MajorId = item,
                                ArticleId = approvingArticleDataSet.Id
                            };
                            _uow.MajorArticleRepository.Insert(majorArticle);
                        }
                        _uow.ArticleRepository.Update(articleToUpdate);

                        int result = await _uow.CommitAsync();

                        if (result > 0)
                        {
                            ApprovingArticleDataSet successApproving = _mapper.Map<ApprovingArticleDataSet>(articleToUpdate);
                            if (successApproving.University == null)
                                successApproving.University = new List<int>();
                            foreach (var item in articleToUpdate.UniversityArticles)
                            {
                                successApproving?.University?.Add(item.UniversityId);
                            }

                            if (successApproving.Major == null)
                                successApproving.Major = new List<int>();
                            foreach (var item in articleToUpdate.MajorArticles)
                            {
                                successApproving?.Major?.Add(item.MajorId);
                            }


                            response = new Response<ApprovingArticleDataSet>(successApproving)
                            {
                                Message = "Duyệt tin tức thành công!"
                            };

                            //FIND USER TO SEND NOTI
                            if (oldStatus != approvingArticleDataSet.Status && approvingArticleDataSet.Status == Consts.STATUS_PUBLISHED
                                && DateTime.Compare(JWTUtils.GetCurrentTimeInVN(), (DateTime)approvingArticleDataSet.PublicFromDate) >= 0)
                            {
                                Dictionary<int, Models.User> dictionaryUsers = new Dictionary<int, Models.User>();
                                IEnumerable<MajorArticle> majorArticles = await _uow.MajorArticleRepository.Get(filter: m => m.ArticleId == articleToUpdate.Id);
                                IEnumerable<UniversityArticle> universityArticles = await _uow.UniversityArticleRepository.Get(filter: u => u.ArticleId == articleToUpdate.Id);
                                IEnumerable<int> majorIds = majorArticles.Select(s => s.MajorId);
                                IEnumerable<int> universityIds = universityArticles.Select(s => s.UniversityId);

                                IEnumerable<Models.FollowingDetail> followingDetails = await _uow.FollowingDetailRepository
                                    .Get(filter: f => f.IsReceiveNotification == true && f.Status == Consts.STATUS_ACTIVE,
                                    includeProperties: "User,EntryMark,EntryMark.MajorSubjectGroup,EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail");
                                if (followingDetails.Count() > 0)
                                {
                                    foreach (var followingDetail in followingDetails)
                                    {
                                        int majorId = followingDetail.EntryMark.MajorSubjectGroup.MajorId;
                                        int universityId = followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.UniversityId;
                                        if (majorIds.Contains(majorId) || universityIds.Contains(universityId))
                                        {
                                            if (!dictionaryUsers.ContainsKey(followingDetail.UserId))
                                            {
                                                dictionaryUsers.Add(followingDetail.UserId, followingDetail.User);
                                            }
                                        }

                                    }
                                }
                                //FIND USER TO SEND NOTI
                                //RESULT
                                IEnumerable<Models.User> users = dictionaryUsers.Values;
                                var messages = new List<Message>();
                                List<Models.Notification> notifications = new List<Models.Notification>();
                                foreach (Models.User userEl in users)
                                {
                                    Models.Notification notification = new Models.Notification()
                                    {
                                        DateRecord = JWTUtils.GetCurrentTimeInVN(),
                                        Data = articleToUpdate.Id.ToString(),
                                        Message = articleToUpdate.Title,
                                        IsRead = false,
                                        Type = NotificationTypes.NewArticle,
                                        UserId = userEl.Id
                                    };
                                    notifications.Add(notification);
                                    messages.Add(new Message()
                                    {
                                        Notification = new FirebaseAdmin.Messaging.Notification()
                                        {
                                            Title = "Bạn có bài viết mới!",
                                            Body = notification.Message
                                        },
                                        Data = new Dictionary<string, string>()
                                        {
                                            {"type" , notification.Type.ToString()},
                                            {"message" , notification.Message},
                                            {"data" , notification.Data},
                                        },
                                        Topic = userEl.Id.ToString()
                                    });
                                }
                                _uow.NotificationRepository.InsertRange(notifications);
                                await _uow.CommitAsync();
                                #pragma warning disable
                                _firebaseService.SendBatchMessage(messages);
                                #pragma warning restore

                            }
                        }
                    }
                }
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

        public async Task<Response<List<int>>> GetUnApprovedArticleIds()
        {
            Response<List<int>> result = new Response<List<int>>();

            try
            {
                IEnumerable<Models.Article> articles = await _uow.ArticleRepository
               .Get(filter: a => a.Status == 0,
                   orderBy: o => o.OrderByDescending(a => a.PostedDate));

                if (articles == null)
                {
                    result.Message = "Tất cả các tin tức đã được duyệt!";
                }

                result.Data = articles.Select(a => a.Id).ToList();
                result.Succeeded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return result;
        }

        public async Task<Response<List<AdminArticleCollapseDataSet>>> GetTopArticlesForAdmin()
        {
            Response<List<AdminArticleCollapseDataSet>> response = new Response<List<AdminArticleCollapseDataSet>>();

            try
            {
                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();

                IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                    .Get(filter: a => a.Status == Articles.Published
                     && (a.PublicFromDate != null && a.PublicFromDate <= currentDate)
                     && (a.PublicToDate != null && a.PublicToDate >= currentDate)
                     && (a.ImportantLevel != null && a.ImportantLevel > 0),
                    orderBy: o => o.OrderByDescending(a => a.ImportantLevel));

                if (articles.Count() == 0)
                {
                    response.Succeeded = true;
                    response.Message = "Không có tin tức hot nào!";
                }
                else
                {
                    var articleCollapseDataSet = articles.Select(m => _mapper.Map<AdminArticleCollapseDataSet>(m)).ToList();
                    response = new Response<List<AdminArticleCollapseDataSet>>(articleCollapseDataSet);
                }
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

        public async Task<Response<List<HomeArticle>>> GetHomeArticles()
        {
            Response<List<HomeArticle>> response = new Response<List<HomeArticle>>();
            var topArticles = new List<ArticleCollapseDataSet>();
            var todayArticles = new List<ArticleCollapseDataSet>();
            var oldArticles = new List<ArticleCollapseDataSet>();

            try
            {
                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();

                topArticles = (await _uow.ArticleRepository
                .Get(filter: a => a.Status == Articles.Published
                    && (a.PublicFromDate != null && a.PublicFromDate <= currentDate)
                    && (a.PublicToDate != null && a.PublicToDate >= currentDate)
                    && (a.ImportantLevel != null && a.ImportantLevel > 0),
                orderBy: o => o.OrderByDescending(a => a.ImportantLevel)))
                .Select(m => _mapper.Map<ArticleCollapseDataSet>(m)).ToList();
                foreach (var article in topArticles)
                {
                    article.TimeAgo = JWTUtils.CalculateTimeAgo(article.PublicFromDate);
                }

                var date = currentDate.Date;

                todayArticles = (await _uow.ArticleRepository
                .Get(filter: a => a.Status == Articles.Published
                    && (a.PublicFromDate != null && a.PublicFromDate >= date && a.PublicFromDate < date.AddDays(1))
                    && (a.PublicToDate != null && a.PublicToDate >= currentDate),
                orderBy: o => o.OrderByDescending(a => a.PublicFromDate)))
                .Select(m => _mapper.Map<ArticleCollapseDataSet>(m)).ToList();
                foreach (var article in todayArticles)
                {
                    article.TimeAgo = JWTUtils.CalculateTimeAgo(article.PublicFromDate);
                }

                oldArticles = (await _uow.ArticleRepository
                .Get(filter: a => a.Status == Articles.Published
                    && (a.PublicFromDate != null && a.PublicFromDate < date)
                    && (a.PublicToDate != null && a.PublicToDate >= currentDate),
                orderBy: o => o.OrderByDescending(a => a.PublicFromDate)))
                .Select(m => _mapper.Map<ArticleCollapseDataSet>(m)).Take(8).ToList();
                foreach (var article in oldArticles)
                {
                    article.TimeAgo = JWTUtils.CalculateTimeAgo(article.PublicFromDate);
                }

                List<HomeArticle> homeArticles = new List<HomeArticle>()
                {
                    new HomeArticle()
                    {
                        Type = HomeArticleTypes.Hot,
                        Articles = topArticles
                    },
                    new HomeArticle()
                    {
                        Type = HomeArticleTypes.Today,
                        Articles = todayArticles
                    },
                    new HomeArticle()
                    {
                        Type = HomeArticleTypes.Past,
                        Articles = oldArticles
                    }
                };

                response = new Response<List<HomeArticle>>(homeArticles.OrderBy(a => a.Type).ToList());
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

        public async Task<Response<List<AdminArticleCollapseDataSet>>> SetTopArticles(List<int> articleIds, string token)
        {
            Response<List<AdminArticleCollapseDataSet>> response = new Response<List<AdminArticleCollapseDataSet>>();
            try
            {
                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();
                IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                   .Get(filter: a => a.Status == Articles.Published
                    && (a.PublicFromDate != null && a.PublicFromDate <= currentDate)
                    && (a.PublicToDate != null && a.PublicToDate >= currentDate));

                List<int> publishedArticleIds = articles.Select(a => a.Id).ToList();
                List<string> invalidArticleTitle = null;

                foreach (var item in articleIds)
                {
                    if (!publishedArticleIds.Contains(item))
                    {
                        string title = (await _uow.ArticleRepository.GetById(item)).Title;
                        if (invalidArticleTitle == null)
                            invalidArticleTitle = new List<string>();
                        invalidArticleTitle.Add(title);
                    }
                }

                if (invalidArticleTitle != null && invalidArticleTitle.Count > 0)
                {
                    if (response.Errors == null)
                        response.Errors = new List<string>();
                    foreach (var item in invalidArticleTitle)
                    {
                        response.Errors.Add("Bài viết: " + item + " không hợp lệ, vui lòng kiểm tra lại!");
                    }
                }
                else
                {
                    var currentTop = await _uow.ArticleRepository.Get(a => a.ImportantLevel > 0);
                    foreach (var item in currentTop)
                    {
                        item.ImportantLevel = 0;
                    }

                    _uow.ArticleRepository.UpdateRange(currentTop);

                    int numberOfUpdate = articleIds.Count();

                    List<Models.Article> articleToUpdate = new List<Models.Article>();
                    foreach (var item in articleIds)
                    {
                        var test = await _uow.ArticleRepository.GetById(item);
                        test.ImportantLevel = numberOfUpdate--;
                        articleToUpdate.Add(test);
                    }

                    _uow.ArticleRepository.UpdateRange(articleToUpdate);

                    int result = await _uow.CommitAsync();
                    if (result > 0)
                    {
                        var articleCollapseDataSet = articleToUpdate.Select(m => _mapper.Map<AdminArticleCollapseDataSet>(m)).ToList();
                        response = new Response<List<AdminArticleCollapseDataSet>>(articleCollapseDataSet);
                    }
                }
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

        public async Task<Response<List<int>>> GetApprovedArticleIds()
        {
            Response<List<int>> result = new Response<List<int>>();

            try
            {
                IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                .Get(filter: a => a.Status == Consts.STATUS_ACTIVE,
                    orderBy: o => o.OrderByDescending(a => a.PostedDate));

                if (articles == null)
                {
                    result.Message = "Chưa có tin tức nào được duyệt!";
                }

                result.Data = articles.Select(a => a.Id).ToList();
                result.Succeeded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return result;
        }

        public async Task<Response<List<AdminArticleCollapseDataSet>>> GetListArticleNotPagination(AdminArticleFilter articleFilter)
        {
            Response<List<AdminArticleCollapseDataSet>> result = new Response<List<AdminArticleCollapseDataSet>>();

            try
            {
                Expression<Func<Models.Article, bool>> filter = null;

                filter = a => (string.IsNullOrEmpty(articleFilter.Search) || a.Title.Contains(articleFilter.Search))
                && (articleFilter.PublicFromDate == null || articleFilter.PublicFromDate == DateTime.MinValue
                || a.PublicFromDate >= articleFilter.PublicFromDate)
                && (articleFilter.PublicToDate == null || articleFilter.PublicToDate == DateTime.MinValue
                || a.PublicToDate <= articleFilter.PublicToDate)
                && (articleFilter.PostedDate == null || articleFilter.PostedDate == DateTime.MinValue
                || a.PostedDate.Value.Date == articleFilter.PostedDate.Date)
                && (articleFilter.ImportantLevel == null || a.ImportantLevel == articleFilter.ImportantLevel)
                && (string.IsNullOrEmpty(articleFilter.PublishedPage) || a.PublishedPage.Equals(articleFilter.PublishedPage))
                && (articleFilter.Status < 0 || a.Status == articleFilter.Status);

                Func<IQueryable<Models.Article>, IOrderedQueryable<Models.Article>> order = null;
                switch (articleFilter.Order ?? 0)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.CrawlerDate);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.CrawlerDate);
                        break;
                    case 2:
                        order = order => order.OrderBy(a => a.Title);
                        break;
                    case 3:
                        order = order => order.OrderByDescending(a => a.Title);
                        break;
                    case 4:
                        order = order => order.OrderBy(a => a.PostedDate);
                        break;
                    case 5:
                        order = order => order.OrderByDescending(a => a.PostedDate);
                        break;
                    case 6:
                        order = order => order.OrderBy(a => a.ImportantLevel);
                        break;
                    case 7:
                        order = order => order.OrderByDescending(a => a.ImportantLevel);
                        break;
                }


                IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                    .Get(filter: filter, orderBy: order);

                if (articles.Count() == 0)
                {
                    result.Succeeded = true;
                    result.Message = "Không có tin tức nào để hiển thị!";
                }
                else
                {
                    var articleCollapseDataSet = articles.Select(m => _mapper.Map<AdminArticleCollapseDataSet>(m)).ToList();
                    result.Succeeded = true;
                    result.Data = articleCollapseDataSet;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return result;
        }

        public async Task<PagedResponse<List<ArticleCollapseDataSet>>> GetListFollowingArticle(PaginationFilter validFilter, string token)
        {
            PagedResponse<List<ArticleCollapseDataSet>> response = new PagedResponse<List<ArticleCollapseDataSet>>();

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

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                    return response;
                }

                //GET CURRENT TIME
                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();

                //GET LIST ARTICLE
                Dictionary<int, Models.Article> articlesByUser = new Dictionary<int, Models.Article>();
                IEnumerable<Models.FollowingDetail> followingDetailPerUser = await _uow.FollowingDetailRepository
                .Get(filter: f => f.IsReceiveNotification == true && f.UserId == user.Id && f.Status == Consts.STATUS_ACTIVE,
                includeProperties: "EntryMark,EntryMark.MajorSubjectGroup,EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail");
                if (followingDetailPerUser.Count() > 0)
                {
                    foreach (var followingDetail in followingDetailPerUser)
                    {
                        int majorId = followingDetail.EntryMark.MajorSubjectGroup.MajorId;
                        int universityId = followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.UniversityId;

                        IEnumerable<Models.Article> articlesByMajor = (await _uow.MajorArticleRepository.
                            Get(filter: m => m.MajorId == majorId, includeProperties: "Article")).Select(s => s.Article).
                            Where(a => a.Status == Articles.Published && a.PublicFromDate != null && a.PublicToDate != null
                    && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0);


                        foreach (var article in articlesByMajor)
                        {
                            if (!articlesByUser.ContainsKey(article.Id))
                            {
                                articlesByUser.Add(article.Id, article);
                            }
                        }
                        IEnumerable<Models.Article> articlesByUniversity = (await _uow.UniversityArticleRepository.
                            Get(filter: m => m.UniversityId == universityId, includeProperties: "Article"))
                            .Select(s => s.Article).
                            Where(a => a.Status == Articles.Published && a.PublicFromDate != null && a.PublicToDate != null
                    && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0);


                        foreach (var article in articlesByUniversity)
                        {
                            if (!articlesByUser.ContainsKey(article.Id))
                            {
                                articlesByUser.Add(article.Id, article);
                            }
                        }
                    }
                }

                IEnumerable<Models.Article> articlesResult = articlesByUser.Values.OrderByDescending(o => o.PostedDate)
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize).Take(validFilter.PageSize).ToList();


                if (articlesResult.Count() <= 0)
                {
                    response.Succeeded = true;
                    response.Message = "Không có tin tức nào để hiển thị!";
                }
                else
                {
                    var articleCollapseDataSet = articlesResult.Select(m => _mapper.Map<ArticleCollapseDataSet>(m)).ToList();
                    var totalRecords = articlesByUser.Count();
                    response = PaginationHelper.CreatePagedReponse(articleCollapseDataSet, validFilter, totalRecords);
                }
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

        public async Task<Response<AdminArticleCollapseDataSet>> CreateNewArticle(CreateArticleParam createArticleParam, string token)
        {
            Response<AdminArticleCollapseDataSet> response = new Response<AdminArticleCollapseDataSet>();

            //Update Block
            using var tran = _uow.GetTransaction();
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

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                    return response;
                }
                if (createArticleParam.Content == null || createArticleParam.Content.Trim().Length == 0 ||
                    createArticleParam.Title == null || createArticleParam.Title.Trim().Length == 0 ||
                    createArticleParam.ShortDescription == null || createArticleParam.ShortDescription.Trim().Length == 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tiêu đề/ Nội dung không được để trống!");
                    return response;
                }
                Models.Article article = new Models.Article
                {
                    Title = createArticleParam.Title,
                    Content = await FirebaseHelper.UploadBase64ImgToFirebase(createArticleParam.Content),
                    ShortDescription = createArticleParam.ShortDescription,
                    Censor = user.Id,
                    CrawlerDate = JWTUtils.GetCurrentTimeInVN(),
                    Status = Articles.New,
                };
                IFormFile postImage = createArticleParam.PostImage;
                if (postImage != null)
                {
                    if (Consts.IMAGE_EXTENSIONS.Contains(Path.GetExtension(postImage.FileName).ToUpperInvariant()))
                    {

                        using (var ms = new MemoryStream())
                        {
                            postImage.CopyTo(ms);
                            ms.Position = 0;
                            if (ms != null && ms.Length > 0)
                            {
                                var auth = new FirebaseAuthProvider(new FirebaseConfig(Consts.API_KEY));
                                var firebaseAuth = await auth.SignInWithEmailAndPasswordAsync(Consts.AUTH_MAIL, Consts.AUTH_PASSWORD);

                                // you can use CancellationTokenSource to cancel the upload midway
                                var cancellation = new CancellationTokenSource();

                                var task = new FirebaseStorage(
                                    Consts.BUCKET,
                                    new FirebaseStorageOptions
                                    {
                                        ThrowOnCancel = true, // when you cancel the upload, exception is thrown. By default no exception is thrown
                                        AuthTokenAsyncFactory = () => Task.FromResult(firebaseAuth.FirebaseToken),
                                    })
                                    .Child(Consts.ARTICLE_FOLDER)
                                    .Child(DateTime.UtcNow.ToString("yyyyMMddHHmmssFFF") + Path.GetExtension(postImage.FileName))
                                    .PutAsync(ms, cancellation.Token);
                                try
                                {
                                    article.PostImageUrl = await task;
                                }
                                catch
                                {
                                    //up không được ảnh thì lấy ảnh đầu tiên trong bài
                                    article.PostImageUrl = Regex.Match(article.Content, "https://firebase(.*?)(?=\")").Value.ToString();
                                }
                            }

                        }
                    }
                }
                else
                {
                    //không có ảnh lấy ảnh đầu trong bài
                    article.PostImageUrl = Regex.Match(article.Content, "https://firebase(.*?)(?=\")").Value.ToString();
                }
                _uow.ArticleRepository.Insert(article);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Thêm bài viết không thành công, lỗi hệ thống!");
                    return response;
                }
                if (createArticleParam.UniversityIds != null && createArticleParam.UniversityIds.Count > 0)
                {
                    foreach (var id in createArticleParam.UniversityIds)
                    {
                        if (await _uow.UniversityRepository.GetById(id) == null)
                        {

                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Danh sách trường không hợp lệ!");
                            return response;
                        }
                        _uow.UniversityArticleRepository.Insert(new Models.UniversityArticle
                        {
                            ArticleId = article.Id,
                            UniversityId = id
                        });
                    }
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Thêm bài viết không thành công, lỗi hệ thống!");
                        return response;
                    }
                }
                if (createArticleParam.MajorIds != null && createArticleParam.MajorIds.Count > 0)
                {
                    foreach (var id in createArticleParam.MajorIds)
                    {
                        if (await _uow.MajorRepository.GetById(id) == null)
                        {

                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Danh sách ngành không hợp lệ!");
                            return response;
                        }
                        _uow.MajorArticleRepository.Insert(new Models.MajorArticle
                        {
                            ArticleId = article.Id,
                            MajorId = id
                        });
                    }
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Thêm bài viết không thành công, lỗi hệ thống!");
                        return response;
                    }
                }
                response.Data = _mapper.Map<AdminArticleCollapseDataSet>(article);
                response.Succeeded = true;
                tran.Commit();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                tran.Rollback();
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<AdminArticleDetailDataSet>> UpdateArticle(UpdateArticleParam updateArticleParam, string token)
        {
            Response<AdminArticleDetailDataSet> response = new Response<AdminArticleDetailDataSet>();
            // status 0 hoặc 1 hoặc 5 mới được update
            // chỉ được update nội dung
            //Update Block
            using var tran = _uow.GetTransaction();
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

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                    return response;
                }

                if (updateArticleParam.Content == null || updateArticleParam.Content.Trim().Length == 0 ||
                    updateArticleParam.Title == null || updateArticleParam.Title.Trim().Length == 0 ||
                    updateArticleParam.ShortDescription == null || updateArticleParam.ShortDescription.Trim().Length == 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tiêu đề/ Nội dung không được để trống!");
                    return response;
                }

                Models.Article article = await _uow.ArticleRepository.GetById(updateArticleParam.Id);
                if (article == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bài viết không tồn tại!");
                    return response;
                }
                if (article.RootUrl != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn không thể chỉnh sửa nội dung bài viết này!");
                    return response;
                }
                if (article.Status != Articles.New && article.Status != Articles.Considered)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không thể cập nhật bài viết đang được đăng!");
                    return response;
                }
                //update nội dung
                article.Title = updateArticleParam.Title;
                article.Content = await FirebaseHelper.UploadBase64ImgToFirebase(updateArticleParam.Content);
                article.ShortDescription = updateArticleParam.ShortDescription;
                article.Censor = user.Id;

                IFormFile postImage = updateArticleParam.PostImage;
                if (postImage != null)
                {
                    if (Consts.IMAGE_EXTENSIONS.Contains(Path.GetExtension(postImage.FileName).ToUpperInvariant()))
                    {

                        using (var ms = new MemoryStream())
                        {
                            postImage.CopyTo(ms);
                            ms.Position = 0;
                            if (ms != null && ms.Length > 0)
                            {
                                var auth = new FirebaseAuthProvider(new FirebaseConfig(Consts.API_KEY));
                                var firebaseAuth = await auth.SignInWithEmailAndPasswordAsync(Consts.AUTH_MAIL, Consts.AUTH_PASSWORD);

                                // you can use CancellationTokenSource to cancel the upload midway
                                var cancellation = new CancellationTokenSource();

                                var task = new FirebaseStorage(
                                    Consts.BUCKET,
                                    new FirebaseStorageOptions
                                    {
                                        ThrowOnCancel = true, // when you cancel the upload, exception is thrown. By default no exception is thrown
                                        AuthTokenAsyncFactory = () => Task.FromResult(firebaseAuth.FirebaseToken),
                                    })
                                    .Child(Consts.ARTICLE_FOLDER)
                                    .Child(DateTime.UtcNow.ToString("yyyyMMddHHmmssFFF") + Path.GetExtension(postImage.FileName))
                                    .PutAsync(ms, cancellation.Token);
                                try
                                {
                                    article.PostImageUrl = await task;
                                }
                                catch (Exception ex)
                                {
                                    _log.Error(ex.ToString());
                                    //up k được thì lấy hình cũ
                                    //hình cũ k có luôn thì lấy hình đầu tiên trong bài
                                    if (String.IsNullOrEmpty(article.PostImageUrl))
                                    {
                                        article.PostImageUrl = Regex.Match(article.Content, "https://firebase(.*?)(?=\")").Value.ToString();
                                    }
                                }
                            }

                        }
                    }
                }


                _uow.ArticleRepository.Update(article);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Cập nhật bài viết không thành công, lỗi hệ thống!");
                    return response;
                }
                _uow.UniversityArticleRepository.DeleteComposite(filter: u => u.ArticleId == article.Id);
                _uow.MajorArticleRepository.DeleteComposite(filter: u => u.ArticleId == article.Id);
                if (updateArticleParam.UniversityIds != null && updateArticleParam.UniversityIds.Count > 0)
                {
                    foreach (var id in updateArticleParam.UniversityIds)
                    {
                        if (await _uow.UniversityRepository.GetById(id) == null)
                        {

                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Danh sách trường không hợp lệ!");
                            return response;
                        }
                        _uow.UniversityArticleRepository.Insert(new Models.UniversityArticle
                        {
                            ArticleId = article.Id,
                            UniversityId = id
                        });
                    }
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Thêm bài viết không thành công, lỗi hệ thống!");
                        return response;
                    }
                }
                if (updateArticleParam.MajorIds != null && updateArticleParam.MajorIds.Count > 0)
                {
                    foreach (var id in updateArticleParam.MajorIds)
                    {
                        if (await _uow.MajorRepository.GetById(id) == null)
                        {

                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Danh sách ngành không hợp lệ!");
                            return response;
                        }
                        _uow.MajorArticleRepository.Insert(new Models.MajorArticle
                        {
                            ArticleId = article.Id,
                            MajorId = id
                        });
                    }
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Cập nhật bài viết không thành công, lỗi hệ thống!");
                        return response;
                    }
                }

                response.Data = _mapper.Map<AdminArticleDetailDataSet>(article);
                response.Succeeded = true;
                tran.Commit();
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                tran.Rollback();
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public async Task<Response<bool>> UpdateExpireStatus()
        {
            Response<bool> response = new Response<bool>();
            try
            {
                DateTime currentDate = JWTUtils.GetCurrentTimeInVN();

                IEnumerable<Models.Article> articles = await _uow.ArticleRepository.Get(filter: a => a.Status == Consts.STATUS_PUBLISHED &&
                a.PublicToDate != null && DateTime.Compare(currentDate, (DateTime)a.PublicToDate) > 0);
                if (articles.Any())
                {
                    foreach (var article in articles)
                    {
                        article.Status = Articles.Expired;
                        article.ImportantLevel = null;
                    }
                    _uow.ArticleRepository.UpdateRange(articles);
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Cập nhật trạng thái bài viết không thành công, lỗi hệ thống!");
                        return response;
                    }
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
    }
}
