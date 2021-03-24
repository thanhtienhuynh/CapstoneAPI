using CapstoneAPI.Services.Crawler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.CronJobs
{
    public class ArticleCrawlerCronJob : IJob
    {
        private readonly ILogger<ArticleCrawlerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ArticleCrawlerCronJob(ILogger<ArticleCrawlerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // Resolve the Scoped service
                var service = scope.ServiceProvider.GetService<IArticleCrawlerService>();
                _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss} CrawlArticleFromGDTD is working!");
                int gdtdResult = await service.CrawlArticleFromGDTD();
                if (gdtdResult > 0)
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: " + gdtdResult + " new articles was crawled from GDTD!");
                else
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: None new article was crawled from GDTD!");

                _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss} CrawlArticleFromVNExpress is working!");
                int VNExpressResult = await service.CrawlArticleFromVNExpress();
                if (VNExpressResult > 0)
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: " + VNExpressResult + " new articles was crawled from VNExpress!");
                else
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: None new article was crawled from VNExpress!");

            }
        }
    }
}