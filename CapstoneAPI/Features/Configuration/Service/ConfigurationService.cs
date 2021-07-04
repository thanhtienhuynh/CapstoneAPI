using CapstoneAPI.Wrappers;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

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
    }
}
