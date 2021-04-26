using AutoMapper;
using CapstoneAPI.DataSets.Article;
using CapstoneAPI.Filters;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.UriService;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Article
{
    public class ArticleService : IArticleService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        //private readonly IUriService _uriService;
        public ArticleService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            //_uriService = uriService;
        }
        
        public async Task<PagedResponse<List<ArticleCollapseDataSet>>> GetListArticleForGuest(PaginationFilter validFilter, string route)
        {
            PagedResponse<List<ArticleCollapseDataSet>> result;

            DateTime currentDate = DateTime.UtcNow.AddHours(7);

            IEnumerable<Models.Article> articles = await _uow.ArticleRepository
                .Get(filter: a => a.Status == 1 && a.PublicFromDate != null && a.PublicToDate != null
                && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0,
                orderBy: o => o.OrderByDescending(a => a.PostedDate),
                first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);

            var a = articles.Select(m => _mapper.Map<ArticleCollapseDataSet>(m)).ToList();
            var totalRecords = _uow.ArticleRepository
                .Count(filter: a => a.Status == 1 && a.PublicFromDate != null && a.PublicToDate != null
                && DateTime.Compare((DateTime)a.PublicToDate, currentDate) > 0);
            result = PaginationHelper.CreatePagedReponse(a, validFilter, totalRecords, route);
            return result;
        }

        public Task<ArticleDetailDataSet> GetArticleById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
