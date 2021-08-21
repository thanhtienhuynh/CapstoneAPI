using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Configuration.DataSet
{
    public class ConfigParam
    {
        public CrawlExpression CrawlTime { get; set; }
        public CrawlExpression UpdateRankTime { get; set; }
        public CrawlExpression ExpireArticleTime { get; set; }
        public int TestMonths { get; set; }
        public int PassRatio { get; set; }
    }

    public class CrawlExpression
    {
        public int Start { get; set; }
        public int MinStart { get; set; }
        public int Type { get; set; }
        //1: Bn giờ 1 lần
        //2: Bắt đầu lúc
    }
}
