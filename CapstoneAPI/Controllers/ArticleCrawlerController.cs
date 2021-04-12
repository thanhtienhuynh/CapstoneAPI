using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Services.Crawler;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Controllers
{
    [Route("api/v1/article-crawler")]
    [ApiController]
    public class ArticleCrawlerController : ControllerBase
    {
        private readonly IArticleCrawlerService _service;
        public ArticleCrawlerController(IArticleCrawlerService service)
        {
            _service = service;
        }

        [HttpPost("start")]
        public async Task StartCrawler()
        {
            await _service.CrawlArticleFromGDTD();
        }
    }
}
