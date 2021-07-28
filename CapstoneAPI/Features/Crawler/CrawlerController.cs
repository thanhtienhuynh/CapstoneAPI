using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Helpers;
using CapstoneAPI.Services.Crawler;
using CapstoneAPI.Wrappers;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CapstoneAPI.Features.Crawler
{
    [Route("api/v1/crawler")]
    [ApiController]
    public class CrawlerController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ArticleCrawlerService> _logger;

        public CrawlerController(IServiceProvider serviceProvider, ILogger<ArticleCrawlerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpPost("article")]
        public async Task<ActionResult<Response<string>>> GetListArticleForGuest()
        {
            string result = "";
            using (var scope = _serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IArticleCrawlerService>();
                int gdtdResult = await service.CrawlArticleFromGDTD();
                if (gdtdResult > 0)
                {
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: " + gdtdResult + " new articles was crawled from GDTD!");
                    result += $"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: " + gdtdResult + " new articles was crawled from GDTD!";
                }
                else
                {
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: None new article was crawled from GDTD!");
                    result += $"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: None new article was crawled from GDTD!";
                }

                _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss} CrawlArticleFromVNExpress is working!");
                int VNExpressResult = await service.CrawlArticleFromVNExpress();
                if (VNExpressResult > 0)
                {
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: " + VNExpressResult + " new articles was crawled from VNExpress!");
                    result += $"\n{DateTime.Now:dd/MM/yyyy hh:mm:ss}: " + VNExpressResult + " new articles was crawled from VNExpress!";

                }
                else
                {
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: None new article was crawled from VNExpress!");
                    result += $"\n{DateTime.Now:dd/MM/yyyy hh:mm:ss}: None new article was crawled from VNExpress!";
                }
            }

            return Ok(new Response<string>(result));
        }
    }
}
