using CapstoneAPI.Features.Configuration.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Wrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Configuration.Service
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger _log = Log.ForContext<ConfigurationService>();
        public Response<object> GetPaginationConfiguration()
        {
            Response<object> response = new Response<object>();
            try
            {
                var pagingConfigJsonString = File.ReadAllText(@"Configuration\PagingConfiguration.json");
                var pagingConfig = JObject.Parse(pagingConfigJsonString);
                var firstPageValue = pagingConfig.SelectToken("PaginationFilter.firstPage").Value<int>();
                var highestQuantityValue = pagingConfig.SelectToken("PaginationFilter.highestQuantity").Value<int>();
                response = new Response<object>(new
                {
                    firstPage = firstPageValue,
                    highestQuantity = highestQuantityValue
                });
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return response;
        }

        public Response<object> SetPaginationConfiguration(int? firstPage, int? highestQuantity)
        {
            Response<object> response = new Response<object>();
            try
            {
                var pagingConfigJsonString = File.ReadAllText(@"Configuration\PagingConfiguration.json");
                var pagingConfig = Newtonsoft.Json.JsonConvert.DeserializeObject(pagingConfigJsonString) as JObject;
                if (firstPage != null)
                    pagingConfig.SelectToken("PaginationFilter.firstPage").Replace(firstPage.ToString());
                if (highestQuantity != null)
                    pagingConfig.SelectToken("PaginationFilter.highestQuantity").Replace(highestQuantity.ToString());

                string updatedJsonString = pagingConfig.ToString();
                File.WriteAllText(@"Configuration\PagingConfiguration.json", updatedJsonString);
                response.Succeeded = true;

                if (firstPage != null && highestQuantity != null)
                {
                    response.Message = "Cập nhật firstPage và highestQuantity thành công!";
                    return response;
                }
                else if (firstPage != null && highestQuantity == null)
                {
                    response.Message = "Cập nhật firstPage thành công!";
                    return response;
                }
                else if (firstPage == null && highestQuantity != null)
                {
                    response.Message = "Cập nhật highestQuantity thành công!";
                    return response;
                }
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return null;
        }

        public async Task<Response<ConfigParam>> SetAppConfiguration(ConfigParam configuration)
        {
            Response<ConfigParam> response = new Response<ConfigParam>();
            if ((configuration.CrawlTime.Start < 0 || configuration.CrawlTime.Start > 23)
                || (configuration.CrawlTime.Type != CronExporessionType.EachHours && configuration.CrawlTime.Type != CronExporessionType.SpecificHour)
                || (configuration.CrawlTime.MinStart < 0 || configuration.CrawlTime.MinStart > 59)
                || (configuration.UpdateRankTime.Start < 0 || configuration.UpdateRankTime.Start > 23)
                || (configuration.UpdateRankTime.Type != CronExporessionType.EachHours && configuration.UpdateRankTime.Type != CronExporessionType.SpecificHour)
                || (configuration.UpdateRankTime.MinStart < 0 || configuration.UpdateRankTime.MinStart > 59)
                || (configuration.TestMonths <= 0 || configuration.TestMonths > 12)
                || (configuration.ExpireArticleTime.Start < 0 || configuration.ExpireArticleTime.Start > 23)
                || (configuration.ExpireArticleTime.Type != CronExporessionType.EachHours && configuration.ExpireArticleTime.Type != CronExporessionType.SpecificHour)
                || (configuration.ExpireArticleTime.MinStart < 0 || configuration.ExpireArticleTime.MinStart > 59)) {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Cấu hình không hợp lệ!");
                return response;
            }
            try
            {
                var appConfigLines = await File.ReadAllTextAsync(@"Configuration\AppConfig.json");
                var appConfig = Newtonsoft.Json.JsonConvert.DeserializeObject(appConfigLines) as JObject;
                appConfig.SelectToken("UpdateRankTime.Start").Replace(configuration.UpdateRankTime.Start);
                appConfig.SelectToken("UpdateRankTime.Type").Replace(configuration.UpdateRankTime.Type);
                appConfig.SelectToken("CrawlTime.Start").Replace(configuration.CrawlTime.Start);
                appConfig.SelectToken("CrawlTime.Type").Replace(configuration.CrawlTime.Type);
                appConfig.SelectToken("ExpireArticleTime.Start").Replace(configuration.ExpireArticleTime.Start);
                appConfig.SelectToken("ExpireArticleTime.Type").Replace(configuration.ExpireArticleTime.Type);
                appConfig.SelectToken("TestMonths").Replace(configuration.TestMonths);
                string updatedJsonString = appConfig.ToString();
                await File.WriteAllTextAsync(@"Configuration\AppConfig.json", updatedJsonString);
                response.Succeeded = true;
                response.Data = JsonConvert.DeserializeObject<ConfigParam>(updatedJsonString);
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return response;
        }

        public async Task<Response<ConfigParam>> GetAppConfiguration()
        {
            Response<ConfigParam> response = new Response<ConfigParam>();
            try
            {
                var appConfigLines = await File.ReadAllTextAsync(@"Configuration\AppConfig.json");
                var appConfig = Newtonsoft.Json.JsonConvert.DeserializeObject(appConfigLines) as JObject;
                response.Succeeded = true;
                response.Data = JsonConvert.DeserializeObject<ConfigParam>(appConfig.ToString());
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }

            return response;
        }
    }
}
