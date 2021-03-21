using AutoMapper;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Crawler
{
    public class ArticleCrawlerService : IArticleCrawlerService
    {
        private readonly IUnitOfWork _uow;
        private readonly JObject configuration;

        public ArticleCrawlerService(IUnitOfWork uow)
        {
            _uow = uow;
            string path = Path.Combine(Path
                .GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Configuration\ArticleCrawlerConfiguration.json");
            configuration = JObject.Parse(File.ReadAllText(path));
        }
        public async Task<int> CrawlArticleFromGDTD()
        {
            string pageLink = configuration.SelectToken("GDTD.pageLink").ToString();
            string url = configuration.SelectToken("GDTD.url").ToString();
            try
            {
                HtmlDocument htmlDocument = await CrawlerHelper.GetHtmlDocument(url);

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
                    bool isExist = listArticleDB.Any(articleDB => articleDB.RootUrl.Equals(link));
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

                int result = await GetGDTDArticlesDetails(articles);
                return result;

            }
            catch (Exception e)
            {
            }
            return 0;
        }

        private async Task<int> GetGDTDArticlesDetails(List<Article> articles)
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
            return articles.Count();
        }

        public async Task<int> CrawlArticleFromVNExpress()
        {
            string pageLink = configuration.SelectToken("VNExpress.pageLink").ToString();
            var url = configuration.SelectToken("VNExpress.url").ToString();
            try
            {
                HtmlDocument htmlDocument = await CrawlerHelper.GetHtmlDocument(url);

                var articlesDiv = htmlDocument.GetElementbyId(configuration.SelectToken("VNExpress.articlesDivId").ToString());

                Dictionary<string, string> listArticles = new Dictionary<string, string>();

                var newsDetails = articlesDiv.Descendants(
                    configuration.SelectToken("VNExpress.articleDetailDivs.name").ToString())
                    .Where(node => !string.IsNullOrEmpty(node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.articleDetailDivs.attribute").ToString(), "")));

                List<Article> articles = new List<Article>();
                foreach (var news in newsDetails)
                {
                    string timeInSeconds = news.GetAttributeValue(
                        configuration.SelectToken("VNExpress.articleDetailDivs.attribute").ToString(), "");
                    var titleTag = news.Descendants(configuration.SelectToken("VNExpress.titleTag.name").ToString())
                        .Where(node => node.GetAttributeValue(
                            configuration.SelectToken("VNExpress.titleTag.attribute").ToString(), "")
                        .Contains(configuration.SelectToken("VNExpress.titleTag.attributeValue").ToString()));
                    string title = "";
                    string link = "";
                    if (titleTag != null)
                    {
                        title = titleTag.First().InnerText;
                        var linkTag = titleTag.First().Descendants(
                            configuration.SelectToken("VNExpress.linkTag.name").ToString());
                        link = linkTag.First().GetAttributeValue(
                            configuration.SelectToken("VNExpress.linkTag.attribute").ToString(), "");
                    }

                    IEnumerable<Article> listArticleDB = (await _uow.ArticleRepository.Get(filter: article => article.PublishedPage.Equals(pageLink)));

                    bool a = listArticleDB.Any(test => test.RootUrl.Equals(link));
                    bool isExist = (listArticleDB.Where(articleDB => articleDB.RootUrl.Equals(link)).Count() > 0);
                    if (!isExist)
                    {
                        string shortDescription = "";
                        var descriptionTag = news.Descendants(
                            configuration.SelectToken("VNExpress.shortDescription.name").ToString())
                            .Where(node => node.GetAttributeValue(
                                configuration.SelectToken("VNExpress.shortDescription.attribute").ToString(), "")
                            .Contains(configuration.SelectToken("VNExpress.shortDescription.attributeValue").ToString()));
                        if (descriptionTag != null)
                        {
                            shortDescription = descriptionTag.First().InnerText;
                        }


                        string postImgUrl = "";

                        var imgUrl = news.Descendants(configuration.SelectToken("VNExpress.imgUrlTag.name").ToString())
                            .Where(node => node.GetAttributeValue(
                                configuration.SelectToken("VNExpress.imgUrlTag.containAttribute").ToString(), "")
                            .Contains(configuration.SelectToken("VNExpress.imgUrlTag.containAttributeValue").ToString()));

                        if (imgUrl.Count() > 0)
                        {
                            postImgUrl = imgUrl.First().GetAttributeValue(
                                configuration.SelectToken("VNExpress.imgUrlTag.attribute").ToString(), "");
                        }

                        DateTime postedDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        if (!string.IsNullOrEmpty(timeInSeconds))
                        {
                            postedDate = postedDate.AddSeconds(long.Parse(timeInSeconds)).ToLocalTime();
                        }
                        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link))
                        {
                            Article article = new Article()
                            {
                                Title = title,
                                PostedDate = postedDate,
                                RootUrl = link,
                                PublishedPage = pageLink,
                                ShortDescription = shortDescription,
                                PostImageUrl = postImgUrl
                            };
                            articles.Add(article);
                        }
                    }
                }

                int result = await VNExpressDetailsCrawler(articles);
                return result;
            } catch
            {
            }
            return 0;
        }

        private async Task<int> VNExpressDetailsCrawler(List<Article> articles)
        {
            foreach (var article in articles)
            {
                HtmlDocument htmlDocument = await CrawlerHelper.GetHtmlDocument(article.RootUrl);

                var headerConfig = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.articleDetails.headerTag.name").ToString()).First();

                var detail = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.articleDetails.detailTag.name").ToString())
                   .Where(node => node.GetAttributeValue(
                       configuration.SelectToken("VNExpress.articleDetails.detailTag.attribute").ToString(), "")
                   .Contains(configuration.SelectToken("VNExpress.articleDetails.detailTag.attributeValue").ToString())).First();

                var content = detail.ParentNode.ParentNode;
                content.RemoveAllChildren();
                content.AppendChild(detail);

                var related = detail.Descendants(
                    configuration.SelectToken("VNExpress.articleDetails.detailTag.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.articleDetails.detailTag.attribute").ToString(), "")
                    .Contains(configuration.SelectToken("VNExpress.articleDetails.detailTag.attributeValue").ToString()));

                if (related.Count() > 0)
                {
                    related.First().ParentNode.RemoveChild(related.First());
                }

                article.HeaderConfig = headerConfig.InnerHtml.Trim();
                article.Status = 1;
                article.CrawlerDate = DateTime.Now;
                article.Content = detail.InnerHtml.Trim();

            }

            _uow.ArticleRepository.InsertRange(articles);
            await _uow.CommitAsync();
            return articles.Count();
        }

    }
}
