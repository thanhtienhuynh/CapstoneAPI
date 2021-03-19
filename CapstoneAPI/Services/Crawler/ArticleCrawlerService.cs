using AutoMapper;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Crawler
{
    public class ArticleCrawlerService : IArticleCrawlerService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly JObject configuration;

        public ArticleCrawlerService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            string path = Path.Combine(Path
                .GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Configuration\ArticleCrawlerConfiguration.json");
            configuration = JObject.Parse(File.ReadAllText(path));
        }
        public async Task CrawlArticleFromGDTD()
        {
            string pageLink = configuration.SelectToken("GDTD.pageLink").ToString();
            string url = configuration.SelectToken("GDTD.url").ToString();
            try
            {
                HtmlDocument htmlDocument = await CrawlerHelper.GetHtmlDocument(url);
                CapstoneDBContext context = new CapstoneDBContext();

                IEnumerable<Article> listArticleDB = (await _uow.ArticleRepository.Get(filter: article => article.PublishedPage.Equals(pageLink)));
                
                Dictionary<string, string> listArticles = new Dictionary<string, string>();

                var articlesDiv = htmlDocument.GetElementbyId(configuration.SelectToken("GDTD.articlesDivId").ToString());
                var newsDetails = articlesDiv.SelectNodes(configuration.SelectToken("GDTD.articleDetailDivs").ToString());

                List<Article> articles = new List<Article>();
                foreach (var news in newsDetails)
                {
                    string title = news.GetAttributeValue("title", "");
                    string link = news.GetAttributeValue("href", "");
                    link = pageLink + link;
                    bool a = listArticleDB.Any(test => test.RootUrl.Equals(link));
                    bool isExist = (listArticleDB.Where(articleDB => articleDB.RootUrl.Equals(link)).Count() > 0);
                    if (!isExist)
                    {
                        var time = news.Descendants(configuration.SelectToken("GDTD.timeTag.name").ToString())
                            .Where(node => node.GetAttributeValue(
                                configuration.SelectToken("GDTD.timeTag.attribute").ToString(), "")
                            .Contains(configuration.SelectToken("GDTD.timeTag.attibuteValue").ToString()));

                        string imgUrl = news.Descendants(configuration.SelectToken("GDTD.imgUrlTag.name").ToString())
                            .First().GetAttributeValue(configuration.SelectToken("GDTD.imgUrlTag.attribute").ToString(), "");

                        string shortDescription = news.Descendants(
                            configuration.SelectToken("GDTD.shortDescription.name").ToString()).First().InnerText;

                        string sPostedTime = "";
                        DateTime postedDate = DateTime.Today;
                        if (time.Count() > 0)
                        {
                            sPostedTime = time.First().InnerHtml;
                            postedDate = DateTime.ParseExact(sPostedTime, configuration.SelectToken("GDTD.postedDateFormat").ToString(), CultureInfo.CurrentCulture);
                        }
                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link))
                        {
                            Article article = new Article()
                            {
                                Title = title,
                                PostedDate = postedDate,
                                RootUrl = link,
                                PublishedPage = pageLink,
                                PostImageUrl = imgUrl,
                                ShortDescription = shortDescription
                            };
                            articles.Add(article);
                        }
                    }
                }

                await GetGDTDArticlesDetails(articles);
            }
            catch (Exception e)
            {

            }
        }

        private async Task GetGDTDArticlesDetails(List<Article> articles)
        {
            string pageLink = configuration.SelectToken("GDTD.pageLink").ToString();
            foreach (var article in articles)
            {

                HtmlDocument htmlDocument = await CrawlerHelper.GetHtmlDocument(article.RootUrl);

                var headerConfig = htmlDocument.DocumentNode
                    .Descendants(configuration.SelectToken("GDTD.articleDetails.headerTag.name").ToString()).First();
                var webIcon = headerConfig.Descendants(configuration.SelectToken("GDTD.articleDetails.webIcon.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("GDTD.articleDetails.webIcon.attribute").ToString(), "")
                    .Contains(configuration.SelectToken("GDTD.articleDetails.webIcon.attibuteValue").ToString())).First();
                if (webIcon != null)
                {
                    webIcon.ParentNode.RemoveChild(webIcon);
                }
                var styles = headerConfig.Descendants(
                    configuration.SelectToken("GDTD.articleDetails.styleUrls.name").ToString())
                    .Where(node => !node.GetAttributeValue(
                        configuration.SelectToken("GDTD.articleDetails.styleUrls.attribute").ToString(), "")
                    .Contains(article.PublishedPage));
                foreach (var style in styles)
                {
                    string currentValue = style.GetAttributeValue(
                        configuration.SelectToken("GDTD.articleDetails.styleUrls.attribute").ToString(), "");
                    style.SetAttributeValue(
                        configuration.SelectToken("GDTD.articleDetails.styleUrls.attribute").ToString(),
                        article.PublishedPage + currentValue);
                }

                var detail = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("GDTD.articleDetails.detailTag.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("GDTD.articleDetails.detailTag.attribute").ToString(), "").
                    Contains(configuration.SelectToken("GDTD.articleDetails.detailTag.attibuteValue").ToString())).First();


                article.HeaderConfig = headerConfig.InnerHtml.Trim();
                article.Status = 1;
                article.CrawlerDate = DateTime.Now;
                article.Content = detail.InnerHtml.Trim();
            }

            _uow.ArticleRepository.InsertRange(articles);
            await _uow.CommitAsync();
        }

        public Task CrawlArticleFromVNExpress()
        {
            throw new NotImplementedException();
        }
    }
}
