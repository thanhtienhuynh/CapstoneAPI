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

            if (articles.Data.Count == 0)
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
    }
}
