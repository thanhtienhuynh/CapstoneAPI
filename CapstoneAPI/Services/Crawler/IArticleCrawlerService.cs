using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Crawler
{
    public interface IArticleCrawlerService
    {
        Task<int> CrawlArticleFromVNExpress();
        Task<int> CrawlArticleFromGDTD();

    }
}
