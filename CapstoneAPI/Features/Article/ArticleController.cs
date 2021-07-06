﻿using CapstoneAPI.Features.Article.DataSet;
using CapstoneAPI.Features.Article.Service;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Article;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Article
{
    [Route("api/v1/article")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _service;
        public ArticleController(IArticleService service)
        {
            _service = service;
        }

        [HttpGet("all")]
        public async Task<ActionResult<PagedResponse<List<ArticleCollapseDataSet>>>> GetListArticleForGuest([FromQuery] PaginationFilter filter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            PagedResponse<List<ArticleCollapseDataSet>> articles = await _service.GetListArticleForGuest(validFilter);

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }
        [HttpGet("detail/{id}")]
        public async Task<ActionResult<Response<ArticleDetailDataSet>>> GetArticleDetailsForGuest(int id)
        {
            Response<ArticleDetailDataSet> article = await _service.GetArticleById(id);
            if (article == null)
                return NoContent();
            return Ok(article);
        }

        [HttpGet("admin-all")]
        public async Task<ActionResult<PagedResponse<List<ArticleCollapseDataSet>>>> GetListArticleForAdmin([FromQuery] PaginationFilter filter, 
            [FromQuery] AdminArticleFilter articleFilter)
        {
            //string token = Request.Headers["Authorization"];
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            PagedResponse<List<AdminArticleCollapseDataSet>> articles = await _service
                .GetListArticleForAdmin(validFilter, articleFilter);

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }
        [HttpGet("admin-all-not-paging")]
        public async Task<ActionResult<Response<List<ArticleCollapseDataSet>>>> GetListArticleNotPagination([FromQuery] AdminArticleFilter articleFilter)
        {
            //string token = Request.Headers["Authorization"];

            Response<List<AdminArticleCollapseDataSet>> articles = await _service
                .GetListArticleNotPagination(articleFilter);

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }
        [HttpGet("approved-article-ids")]
        public async Task<ActionResult<Response<List<int>>>> GetApprovedArticleIds()
        {
            //string token = Request.Headers["Authorization"];

            Response<List<int>> articles = await _service
                .GetApprovedArticleIds();

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }
        [HttpGet("admin-top")]
        public async Task<ActionResult<Response<List<ArticleCollapseDataSet>>>> GetTopArticlesForAdmin()
        {
            //string token = Request.Headers["Authorization"];

            Response<List<AdminArticleCollapseDataSet>> articles = await _service
                .GetTopArticlesForAdmin();

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }
        [HttpGet("admin-detail/{id}")]
        public async Task<ActionResult<Response<AdminArticleDetailDataSet>>> GetArticleDetailsForAdmin(int id)
        {
            //string token = Request.Headers["Authorization"];
            Response<AdminArticleDetailDataSet> article = await _service.AdminGetArticleById(id);
            if (article == null)
                return NoContent();
            return Ok(article);
        }
        [HttpGet("admin-unapproved-articles")]
        public async Task<ActionResult<Response<List<int>>>> GetUnApprovedArticleIds()
        {
            Response<List<int>> article = await _service.GetUnApprovedArticleIds();
            if (article == null)
                return NoContent();
            return Ok(article);
        }
        [HttpPut()]
        public async Task<ActionResult<Response<ApprovingArticleDataSet>>> ApprovingArticle([FromBody] ApprovingArticleDataSet approvingArticleDataSet)
        {
            string token = Request.Headers["Authorization"];
            Response<ApprovingArticleDataSet> result = await _service.ApprovingArticle(approvingArticleDataSet, token);
            if (result == null)
                return NoContent();
            return Ok(result);
        }
        [HttpPut("top")]
        public async Task<ActionResult<Response<List<ArticleCollapseDataSet>>>> SetTopArticles([FromBody] List<int> articleIds)
        {
            string token = Request.Headers["Authorization"];

            Response<List<AdminArticleCollapseDataSet>> articles = await _service.SetTopArticles(articleIds, token);

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }

        [HttpGet("following-article")]
        public async Task<ActionResult<PagedResponse<List<ArticleCollapseDataSet>>>> GetListFollowingArticle([FromQuery] PaginationFilter filter)
        {
            string token = Request.Headers["Authorization"];
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            PagedResponse<List<ArticleCollapseDataSet>> articles = await _service.GetListFollowingArticle(validFilter,token);

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }

        [HttpPost]
        public async Task<ActionResult<Response<ArticleCollapseDataSet>>> CreateANewArticle([FromForm] CreateArticleParam createArticleParam)
        {
            string token = Request.Headers["Authorization"];

            Response<ArticleCollapseDataSet> result = await _service.CreateNewArticle(createArticleParam, token);
            if(result == null)
            {
                return NoContent();
            }
            return Ok(result);
        }

        [HttpPut("update-article")]
        public async Task<ActionResult<Response<AdminArticleDetailDataSet>>> UpdateArticle([FromForm] UpdateArticleParam updateArticleParam)
        {
            string token = Request.Headers["Authorization"];

            Response<AdminArticleDetailDataSet> result = await _service.UpdateArticle(updateArticleParam, token);
            if (result == null)
            {
                return NoContent();
            }
            return Ok(result);
        }
        [HttpPut("update-exprire-article")]
        public async Task<ActionResult<Response<bool>>> UpdateExpireStatus()
        {
            string token = Request.Headers["Authorization"];

            Response<bool> result = await _service.UpdateExpireStatus();
            if (result == null)
            {
                return NoContent();
            }
            return Ok(result);
        }
    }
}