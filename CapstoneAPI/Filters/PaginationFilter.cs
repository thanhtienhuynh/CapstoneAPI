using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CapstoneAPI.Filters
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        private static string path = Path.Combine(Path
                .GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Configuration\PagingConfiguration.json");
        private readonly JObject configuration = JObject.Parse(File.ReadAllText(path));

        public PaginationFilter()
        {
            var sFirstPage = configuration.SelectToken("PaginationFilter.firstPage").ToString();
            int firstPage = int.Parse(sFirstPage);

            var sHighestQuantity = configuration.SelectToken("PaginationFilter.highestQuantity").ToString();
            int highestQuantity = int.Parse(sHighestQuantity);
            this.PageNumber = firstPage;
            this.PageSize = highestQuantity;
        }
        public PaginationFilter(int pageNumber, int pageSize)
        {
            var sFirstPage = configuration.SelectToken("PaginationFilter.firstPage").ToString();
            int firstPage = int.Parse(sFirstPage);

            var sHighestQuantity = configuration.SelectToken("PaginationFilter.highestQuantity").ToString();
            int highestQuantity = int.Parse(sHighestQuantity);

            this.PageNumber = pageNumber < firstPage ? firstPage : pageNumber;
            this.PageSize = pageSize > highestQuantity ? highestQuantity : pageSize;
        }
    }
}
