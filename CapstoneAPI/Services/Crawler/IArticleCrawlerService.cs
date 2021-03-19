using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Crawler
{
    public interface IArticleCrawlerService
    {
        Task CrawlArticleFromVNExpress();
        Task CrawlArticleFromGDTD();

    }
}
