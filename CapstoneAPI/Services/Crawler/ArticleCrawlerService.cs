using AutoMapper;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Quartz;
using Serilog;
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
        private readonly ILogger _log = Log.ForContext<ArticleCrawlerService>();

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

                IEnumerable<Models.Article> listArticleDB = (await _uow.ArticleRepository.Get(filter: article => article.PublishedPage.Equals(pageLink)));

                Dictionary<string, string> listArticles = new Dictionary<string, string>();

                var articlesDiv = htmlDocument.GetElementbyId(configuration.SelectToken("GDTD.articlesDivId").ToString());
                var newsDetails = articlesDiv.SelectNodes(configuration.SelectToken("GDTD.articleDetailDivs").ToString());

                List<Models.Article> articles = new List<Models.Article>();
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
                            Models.Article article = new Models.Article()
                            {
                                Title = title?.Trim(),
                                PostedDate = postedDate,
                                RootUrl = link?.Trim(),
                                PublishedPage = pageLink?.Trim(),
                                PostImageUrl = imgUrl?.Trim(),
                                ShortDescription = shortDescription?.Trim(),
                                ImportantLevel = 0
                            };
                            articles.Add(article);
                        }
                    }
                }

                int result = await GetGDTDArticlesDetails(articles);
                return result;

            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
            return 0;
        }

        private async Task<int> GetGDTDArticlesDetails(List<Models.Article> articles)
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

                double currentTimeZone = 7;

                article.HeaderConfig = headerConfig.InnerHtml.Trim();
                article.Status = 0;
                article.CrawlerDate = DateTime.UtcNow.AddHours(currentTimeZone);
                article.Content = detail.InnerHtml.Trim();
            }

            _uow.ArticleRepository.InsertRange(articles);
            await _uow.CommitAsync();
            return articles.Count();
        }

        public async Task<int> CrawlArticleFromVNExpress()
        {
            string pageLink = configuration.SelectToken("VNExpress.pageLink").ToString();
            string url = configuration.SelectToken("VNExpress.url").ToString();
            try
            {
                HtmlDocument htmlDocument = await CrawlerHelper.GetHtmlDocument(url);

                IEnumerable<Models.Article> listArticleDB = (await _uow.ArticleRepository
                    .Get(filter: article => article.PublishedPage.Equals(pageLink)));
                List<Models.Article> articles = new List<Models.Article>();

                var articleTop = htmlDocument.DocumentNode.Descendants(configuration
                    .SelectToken("VNExpress.topArticleTag.name").ToString())
                    .Where(node => node.GetAttributeValue(configuration
                    .SelectToken("VNExpress.topArticleTag.attribute").ToString(), string.Empty)
                        .Contains(configuration.SelectToken("VNExpress.topArticleTag.attributeValue").ToString()))?.First();
                string articleTopLink = articleTop.Descendants(
                    configuration.SelectToken("VNExpress.topArticleTag.titleTag.name").ToString())
                        .Where(node => node.GetAttributeValue(
                            configuration.SelectToken("VNExpress.topArticleTag.titleTag.attribute").ToString(), string.Empty)
                            .Contains(configuration.SelectToken("VNExpress.topArticleTag.titleTag.attributeValue")
                            .ToString()))?.First().Descendants(
                                configuration.SelectToken("VNExpress.topArticleTag.titleTag.link.name")
                            .ToString())?.First().GetAttributeValue(
                                configuration.SelectToken("VNExpress.topArticleTag.titleTag.link.attribute").ToString(), "")?.Trim();

                bool isExistTopArticle = listArticleDB.Where(article => article.RootUrl.Equals(articleTopLink)).Any();
                if (!isExistTopArticle && !string.IsNullOrEmpty(articleTopLink))
                {
                    articles.Add(new Article()
                    {
                        RootUrl = articleTopLink?.Trim(),
                        PublishedPage = pageLink?.Trim(),
                        ImportantLevel = 0
                    });
                }

                var subNewTops = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.subTopTag.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.subTopTag.attribute").ToString(), string.Empty)
                        .Equals(configuration.SelectToken("VNExpress.subTopTag.attributeValue").ToString()))
                    .First().Descendants(configuration.SelectToken("VNExpress.subTopTag.selection").ToString());

                foreach (var subNewTop in subNewTops)
                {
                    string link = subNewTop.Descendants(
                        configuration.SelectToken("VNExpress.subTopTag.titleTag.name").ToString())
                        .Where(node => node.GetAttributeValue(
                            configuration.SelectToken("VNExpress.subTopTag.titleTag.attribute").ToString(), string.Empty).Equals(
                            configuration.SelectToken("VNExpress.subTopTag.titleTag.attributeValue").ToString()))
                        .First().Descendants(
                        configuration.SelectToken("VNExpress.subTopTag.titleTag.link.name").ToString())?.First()
                        .GetAttributeValue(configuration.SelectToken("VNExpress.subTopTag.titleTag.link.attribute")
                        .ToString(), "")?.Trim();
                    bool isExist = listArticleDB.Where(article => article.RootUrl.Equals(link)).Any();
                    if (!isExist && !string.IsNullOrEmpty(link))
                    {
                        articles.Add(new Article()
                        {
                            RootUrl = link?.Trim(),
                            PublishedPage = pageLink?.Trim(),
                            ImportantLevel = 0
                        });
                    }
                }

                var newsCommons = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.commonTag.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.commonTag.attribute").ToString(), string.Empty)
                        .Contains(configuration.SelectToken("VNExpress.commonTag.attributeValue").ToString()))
                    ?.First().Descendants(configuration.SelectToken("VNExpress.commonTag.selection").ToString());

                foreach (var newCommon in newsCommons)
                {
                    bool isNotAdNews = newCommon.Descendants(
                        configuration.SelectToken("VNExpress.commonTag.titleTag.name").ToString())
                        .Where(node => node.GetAttributeValue(
                            configuration.SelectToken("VNExpress.commonTag.titleTag.attribute").ToString(), string.Empty).Equals(
                            configuration.SelectToken("VNExpress.commonTag.titleTag.attributeValue").ToString())).Count() > 0;

                    if (isNotAdNews)
                    {
                        string link = newCommon.Descendants(
                            configuration.SelectToken("VNExpress.commonTag.titleTag.name").ToString())
                            .Where(node => node.GetAttributeValue(
                                configuration.SelectToken("VNExpress.commonTag.titleTag.attribute").ToString(), string.Empty).Equals(
                                configuration.SelectToken("VNExpress.commonTag.titleTag.attributeValue").ToString()))
                            .First().Descendants(
                            configuration.SelectToken("VNExpress.commonTag.titleTag.link.name").ToString()).First().GetAttributeValue(
                            configuration.SelectToken("VNExpress.commonTag.titleTag.link.attribute").ToString(), string.Empty)?.Trim();
                        bool isExist = listArticleDB.Where(article => article.RootUrl.Equals(link)).Any();
                        if (!isExist && !string.IsNullOrEmpty(link))
                        {
                            articles.Add(new Article()
                            {
                                RootUrl = link?.Trim(),
                                PublishedPage = pageLink?.Trim(),
                                ImportantLevel = 0
                            });
                        }
                    }
                }

                var articlesDivs = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.normalTag.name").ToString())
                   .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.normalTag.attribute").ToString(), string.Empty).Equals(
                       configuration.SelectToken("VNExpress.normalTag.attributeValue").ToString()));

                foreach (var articlesDiv in articlesDivs)
                {

                    var newsDetails = articlesDiv.Descendants(
                       configuration.SelectToken("VNExpress.normalTag.articleTag.name").ToString())
                        .Where(node => !string.IsNullOrEmpty(node.GetAttributeValue(
                            configuration.SelectToken("VNExpress.normalTag.articleTag.attribute").ToString(), string.Empty)));

                    foreach (var news in newsDetails)
                    {
                        var titleTag = news.Descendants(configuration.SelectToken("VNExpress.normalTag.titleTag.name").ToString())
                            .Where(node => node.GetAttributeValue(
                                configuration.SelectToken("VNExpress.normalTag.titleTag.attribute").ToString(), string.Empty)
                            .Contains(configuration.SelectToken("VNExpress.normalTag.titleTag.attributeValue").ToString()));
                        string link = string.Empty;
                        if (titleTag != null)
                        {
                            var linkTag = titleTag.First().Descendants(
                                configuration.SelectToken("VNExpress.normalTag.titleTag.link.name").ToString());
                            link = linkTag.First().GetAttributeValue(
                                configuration.SelectToken("VNExpress.normalTag.titleTag.link.attribute").ToString(), string.Empty)?.Trim();
                        }

                        bool isExist = (listArticleDB.Where(articleDB => articleDB.RootUrl.Equals(link)).Count() > 0);
                        if (!isExist && !string.IsNullOrEmpty(link))
                        {
                            Models.Article article = new Models.Article()
                            {
                                RootUrl = link?.Trim(),
                                PublishedPage = pageLink?.Trim(),
                                ImportantLevel = 0
                            };
                            articles.Add(article);
                        }
                    }
                }

                int result = await VNExpressDetailsCrawler(articles);
                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
            }
            return 0;
        }

        private async Task<int> VNExpressDetailsCrawler(List<Models.Article> articles)
        {
            foreach (var article in articles)
            {
                HtmlDocument htmlDocument = await CrawlerHelper.GetHtmlDocument(article.RootUrl);

                var headerConfigTag = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.details.headerTag.name").ToString()).First();
                string headerConfig = string.Empty;

                if (headerConfigTag != null)
                {
                    headerConfig = headerConfigTag.InnerText;
                }

                var titleTag = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.details.titleTag.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.details.titleTag.attribute").ToString(), string.Empty).Equals(
                        configuration.SelectToken("VNExpress.details.titleTag.attributeValue").ToString()));
                string title = string.Empty;
                if (titleTag != null)
                {
                    title = titleTag.First().InnerText;
                }

                var descriptionTag = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.details.descriptionTag.name").ToString())
                   .Where(node => node.GetAttributeValue(
                       configuration.SelectToken("VNExpress.details.descriptionTag.attribute").ToString(), string.Empty).Equals(
                       configuration.SelectToken("VNExpress.details.descriptionTag.attributeValue").ToString()));
                string description = string.Empty;
                if (descriptionTag != null)
                {
                    description = descriptionTag.First().InnerText;
                }

                var timeTag = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.details.timeTag.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.details.timeTag.attribute").ToString(), string.Empty).Equals(
                        configuration.SelectToken("VNExpress.details.timeTag.attributeValue").ToString()));
                string timeInSeconds = string.Empty;
                if (timeTag != null && timeTag.Any())
                {
                    timeInSeconds = timeTag.First().GetAttributeValue(
                        configuration.SelectToken("VNExpress.details.timeTag.selection").ToString(), string.Empty);
                }

                DateTime postedDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                if (!string.IsNullOrEmpty(timeInSeconds))
                {
                    postedDate = postedDate.AddSeconds(long.Parse(timeInSeconds)).ToLocalTime();
                }

                var detail = htmlDocument.DocumentNode.Descendants(
                    configuration.SelectToken("VNExpress.details.content.name").ToString())
                   .Where(node => node.GetAttributeValue(
                       configuration.SelectToken("VNExpress.details.content.attribute").ToString(), string.Empty)
                   .Contains(configuration.SelectToken("VNExpress.details.content.attributeValue").ToString())).First();

                var content = detail.ParentNode.ParentNode;
                content.RemoveAllChildren();
                content.AppendChild(detail);

                var imgs = detail.Descendants(configuration.SelectToken("VNExpress.details.imgUrlTag.name").ToString())
                                .Where(node => node.GetAttributeValue(
                                    configuration.SelectToken("VNExpress.details.imgUrlTag.containAttribute").ToString(), "")
                                .Contains(configuration.SelectToken("VNExpress.details.imgUrlTag.containAttributeValue").ToString()));
                UpdateNewSrcInImgTag(imgs);

                var related = detail.Descendants(
                    configuration.SelectToken("VNExpress.details.relatedTag.name").ToString())
                    .Where(node => node.GetAttributeValue(
                        configuration.SelectToken("VNExpress.details.relatedTag.attribute").ToString(), "")
                    .Contains(configuration.SelectToken("VNExpress.details.relatedTag.attributeValue").ToString()));

                if (related.Count() > 0)
                {
                    related.First().ParentNode.RemoveChild(related.First());
                }

                if (imgs != null && imgs.Count() > 0)
                {
                    article.PostImageUrl = imgs.First().GetAttributeValue(
                        configuration.SelectToken("VNExpress.postImgUrlTag.attribute").ToString(), "");
                }
                article.HeaderConfig = headerConfig;
                article.Status = 0;
                article.CrawlerDate = DateTime.UtcNow.AddHours(7);
                article.Content = detail.InnerHtml.Trim();
                article.PostedDate = postedDate;
                article.Title = title;
                article.ShortDescription = description;
            }

            _uow.ArticleRepository.InsertRange(articles);
            int result = await _uow.CommitAsync();

            if (result > 0)
            {
                return articles.Count();
            }

            return 0;
        }

        private void UpdateNewSrcInImgTag(IEnumerable<HtmlNode> imgUrls)
        {
            foreach (var img in imgUrls)
            {
                var dataSrcValue = img.GetAttributeValue("data-src", "");
                img.SetAttributeValue("src", dataSrcValue);
            }
        }

    }
}
