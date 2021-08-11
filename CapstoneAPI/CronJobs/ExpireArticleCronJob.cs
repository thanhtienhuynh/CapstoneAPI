using CapstoneAPI.Features.Article.Service;
using CapstoneAPI.Features.Rank.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.CronJobs
{
    public class ExpireArticleCronJob : IJob
    {
        private readonly ILogger<ArticleService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public ExpireArticleCronJob(ILogger<ArticleService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IArticleService>();
                _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss} Expire article is working!");
                bool result = (await service.UpdateExpireStatus()).Data;
                if (result)
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: Expire article success");
                else
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: Expire article fail");
            }
        }
    }
}
