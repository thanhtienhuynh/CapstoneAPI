using AutoMapper;
using CapstoneAPI.DataSets.Article;
using CapstoneAPI.Filters;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                .Get(filter: a => a.Status == 1 && a.PublicFromDate != null && a.PublicToDate != null
                && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0,
                orderBy: o => o.OrderByDescending(a => a.PostedDate),
                first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);

            if (articles.Count() == 0)
            {
                if (result.Errors == null)
                    result.Errors = new List<string>();
                result.Errors.Add("Không có bài viết nào để hiển thị!");
            }
            else
            {
                var articleCollapseDataSet = articles.Select(m => _mapper.Map<ArticleCollapseDataSet>(m)).ToList();
                var totalRecords = _uow.ArticleRepository
                    .Count(filter: a => a.Status == 1 && a.PublicFromDate != null && a.PublicToDate != null
                    && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0);
                result = PaginationHelper.CreatePagedReponse(articleCollapseDataSet, validFilter, totalRecords);
            }

            return result;
        }
        public async Task<PagedResponse<List<AdminArticleCollapseDataSet>>> GetListArticleForAdmin(PaginationFilter validFilter)
        {
            PagedResponse<List<AdminArticleCollapseDataSet>> result = new PagedResponse<List<AdminArticleCollapseDataSet>>();

            IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                .Get(orderBy: o => o.OrderByDescending(a => a.PostedDate),
                first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);
            if (articles.Count() == 0)
            {
                if (result.Errors == null)
                    result.Errors = new List<string>();
                result.Errors.Add("Không có bài viết nào để hiển thị!");
            }
            else
            {
                var articleCollapseDataSet = articles.Select(m => _mapper.Map<AdminArticleCollapseDataSet>(m)).ToList();
                var totalRecords = _uow.ArticleRepository.Count();
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
                result.Errors.Add("Không thể xem bài viết!");
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

            Models.Article article = await _uow.ArticleRepository.GetFirst(filter: a => a.Id == id);

            if (article == null)
            {
                result = new Response<AdminArticleDetailDataSet>();
                if (result.Errors == null)
                    result.Errors = new List<string>();
                result.Errors.Add("Không thể xem bài viết!");
            }
            else
            {
                var data = _mapper.Map<AdminArticleDetailDataSet>(article);
                result = new Response<AdminArticleDetailDataSet>(data);
            }

            return result;
        }
    }
}
