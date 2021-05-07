using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.DataSets.Article;
using CapstoneAPI.Filters;
using CapstoneAPI.Services.Article;
using CapstoneAPI.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
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
        public async Task<ActionResult<PagedResponse<List<ArticleCollapseDataSet>>>> GetListArticleForAdmin([FromQuery] PaginationFilter filter)
        {
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);

            PagedResponse<List<AdminArticleCollapseDataSet>> articles = await _service.GetListArticleForAdmin(validFilter);

            if (articles == null)
                return NoContent();
            return Ok(articles);
        }

        [HttpGet("admin-detail/{id}")]
        public async Task<ActionResult<Response<AdminArticleDetailDataSet>>> GetArticleDetailsForAdmin(int id)
        {
            Response<AdminArticleDetailDataSet> article = await _service.AdminGetArticleById(id);
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
    }
}
