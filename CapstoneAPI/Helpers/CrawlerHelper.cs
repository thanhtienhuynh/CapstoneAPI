using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;

namespace CapstoneAPI.Helpers
{
    public class CrawlerHelper
    {
        public static async Task<HtmlDocument> GetHtmlDocument(string url)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return htmlDocument;
        }
    }
}
