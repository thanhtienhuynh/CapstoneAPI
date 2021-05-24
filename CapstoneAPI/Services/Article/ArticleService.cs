﻿using AutoMapper;
using CapstoneAPI.DataSets.Article;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Article;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Article
{
    public class ArticleService : IArticleService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly JObject configuration;

        public ArticleService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            string path = Path.Combine(Path
                .GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Configuration\TimeZoneConfiguration.json");
            configuration = JObject.Parse(File.ReadAllText(path));
        }

        public async Task<PagedResponse<List<ArticleCollapseDataSet>>> GetListArticleForGuest(PaginationFilter validFilter)
        {
            PagedResponse<List<ArticleCollapseDataSet>> result = new PagedResponse<List<ArticleCollapseDataSet>>();

            var currentTimeZone = configuration.SelectToken("CurrentTimeZone").ToString();

            DateTime currentDate = DateTime.UtcNow.AddHours(int.Parse(currentTimeZone));

            Expression<Func<Models.Article, bool>> filter = null;
            filter = a => a.Status == 3 && a.PublicFromDate != null && a.PublicToDate != null
                && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0;

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
                var totalRecords = _uow.ArticleRepository
                    .Count(filter: filter);
                result = PaginationHelper.CreatePagedReponse(articleCollapseDataSet, validFilter, totalRecords);
            }

            return result;
        }

        public async Task<PagedResponse<List<AdminArticleCollapseDataSet>>> GetListArticleForAdmin(PaginationFilter validFilter,
            AdminArticleFilter articleFilter)
        {
            PagedResponse<List<AdminArticleCollapseDataSet>> result = new PagedResponse<List<AdminArticleCollapseDataSet>>();

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
            switch (articleFilter.Order)
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
                var totalRecords = _uow.ArticleRepository.Count(filter);
                result = PaginationHelper.CreatePagedReponse(articleCollapseDataSet, validFilter, totalRecords);
            }

            return result;
        }
        public async Task<Response<ArticleDetailDataSet>> GetArticleById(int id)
        {
            var currentTimeZone = configuration.SelectToken("CurrentTimeZone").ToString();
            DateTime currentDate = DateTime.UtcNow.AddHours(int.Parse(currentTimeZone));

            Response<ArticleDetailDataSet> result = null;
            Models.Article article = await _uow.ArticleRepository.GetFirst(filter: a => a.Id == id && a.Status == 1
            && a.PublicFromDate != null && a.PublicToDate != null && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0);
            if (article == null)
            {
                result = new Response<ArticleDetailDataSet>();
                if (result.Errors == null)
                    result.Errors = new List<string>();
                result.Errors.Add("Không thể xem tin tức này!");
            }
            else
            {
                var data = _mapper.Map<ArticleDetailDataSet>(article);
                result = new Response<ArticleDetailDataSet>(data);
            }

            return result;
        }

        public async Task<Response<AdminArticleDetailDataSet>> AdminGetArticleById(int id)
        {
            Response<AdminArticleDetailDataSet> result = null;

            Models.Article article = await _uow.ArticleRepository.GetFirst(filter: a => a.Id == id,
                includeProperties: "UniversityArticles,UniversityArticles.University,MajorArticles,MajorArticles.Article");

            if (article == null)
            {
                result = new Response<AdminArticleDetailDataSet>();
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

            return result;
        }

        public async Task<Response<ApprovingArticleDataSet>> ApprovingArticle(ApprovingArticleDataSet approvingArticleDataSet, string token)
        {
            Response<ApprovingArticleDataSet> response = new Response<ApprovingArticleDataSet>();

            if (token == null || token.Trim().Length == 0)
            {
                if (response.Errors == null)
                    response.Errors = new List<string>();
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Length <= 0)
            {
                if (response.Errors == null)
                    response.Errors = new List<string>();
                response.Errors.Add("Tài khoản của bạn không tồn tại!");
                return response;
            }

            int userId = Int32.Parse(userIdString);
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
                }
                else
                {
                    articleToUpdate.PublicFromDate = approvingArticleDataSet?.PublicFromDate;
                    articleToUpdate.PublicToDate = approvingArticleDataSet?.PublicToDate;
                    articleToUpdate.Status = approvingArticleDataSet?.Status;
                    articleToUpdate.Censor = userId;

                    _uow.UniversityArticleRepository.DeleteComposite(filter: uniArt => uniArt.ArticleId == approvingArticleDataSet.Id);
                    foreach (var item in approvingArticleDataSet.University)
                    {
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
                    }
                }
            }

            return response;
        }

        public async Task<Response<List<int>>> GetUnApprovedArticleIds()
        {
            Response<List<int>> result = new Response<List<int>>();

            IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                .Get(filter: a => a.Status == 0,
                    orderBy: o => o.OrderByDescending(a => a.PostedDate));

            if (articles == null)
            {
                result.Message = "Tất cả các tin tức đã được duyệt!";
            }

            result.Data = articles.Select(a => a.Id).ToList();
            result.Succeeded = true;

            return result;
        }

        public async Task<Response<List<AdminArticleCollapseDataSet>>> GetTopArticlesForAdmin()
        {
            Response<List<AdminArticleCollapseDataSet>> response = null;

            var currentTimeZone = configuration.SelectToken("CurrentTimeZone").ToString();
            DateTime currentDate = DateTime.UtcNow.AddHours(int.Parse(currentTimeZone));

            IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                .Get(filter: a => a.Status == 3
                 && (a.PublicFromDate != null && a.PublicFromDate <= currentDate)
                 && (a.PublicToDate != null && a.PublicToDate >= currentDate)
                 && (a.ImportantLevel != null && a.ImportantLevel > 0),
                orderBy: o => o.OrderByDescending(a => a.ImportantLevel));

            if (articles.Count() == 0)
            {
                response = new Response<List<AdminArticleCollapseDataSet>>
                {
                    Succeeded = true,
                    Message = "Không có tin tức hot nào!"
                };
            }
            else
            {
                var articleCollapseDataSet = articles.Select(m => _mapper.Map<AdminArticleCollapseDataSet>(m)).ToList();
                response = new Response<List<AdminArticleCollapseDataSet>>(articleCollapseDataSet);
            }

            return response;
        }

        public async Task<Response<List<AdminArticleCollapseDataSet>>> SetTopArticles(List<int> articleIds, string token)
        {
            Response<List<AdminArticleCollapseDataSet>> response = null;
            var currentTimeZone = configuration.SelectToken("CurrentTimeZone").ToString();
            DateTime currentDate = DateTime.UtcNow.AddHours(int.Parse(currentTimeZone));
            IEnumerable<Models.Article> articles = await _uow.ArticleRepository
               .Get(filter: a => a.Status == 3
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
                response = new Response<List<AdminArticleCollapseDataSet>>();
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

            return response;
        }

        public async Task<Response<List<int>>> GetApprovedArticleIds()
        {
            Response<List<int>> result = new Response<List<int>>();

            IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                .Get(filter: a => a.Status == 1,
                    orderBy: o => o.OrderByDescending(a => a.PostedDate));

            if (articles == null)
            {
                result.Message = "Chưa có tin tức nào được duyệt!";
            }

            result.Data = articles.Select(a => a.Id).ToList();
            result.Succeeded = true;

            return result;
        }

        public async Task<Response<List<AdminArticleCollapseDataSet>>> GetListArticleNotPagination(AdminArticleFilter articleFilter)
        {
            Response<List<AdminArticleCollapseDataSet>> result = new Response<List<AdminArticleCollapseDataSet>>();

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
            switch (articleFilter.Order)
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

            return result;
        }
    }
}
