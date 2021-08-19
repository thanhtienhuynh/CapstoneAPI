using CapstoneAPI.Features.Configuration.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Configuration.Service
{
    public interface IConfigurationService
    {
        Response<object> GetPaginationConfiguration();
        Response<object> SetPaginationConfiguration(int? firstPage, int? highestQuantity);
        Task<Response<ConfigParam>> SetAppConfiguration(ConfigParam configuration);
        Task<Response<ConfigParam>> GetAppConfiguration();
        Task<Response<int>> GetTestConfiguration();
    }
}
