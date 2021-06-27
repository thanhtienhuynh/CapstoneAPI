using CapstoneAPI.Services.Rank;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.CronJobs
{
    public class RankingCronJob : IJob
    {
        private readonly ILogger<RankService> _logger;
        private readonly IServiceProvider _serviceProvider;
        public RankingCronJob(ILogger<RankService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<IRankService>();
                _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss} Update rank is working!");
                bool result = await service.UpdateRank();
                if (result)
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: Update rank success");
                else
                    _logger.LogInformation($"{DateTime.Now:dd/MM/yyyy hh:mm:ss}: Update rank fail");
            }
        }
    }
}
