using CapstoneAPI.Wrappers;
using System;

namespace CapstoneAPI.Services.Configuration
{
    public interface IConfigurationService
    {
        Response<object> GetPaginationConfiguration();
        Response<object> SetPaginationConfiguration(int? firstPage, int? highestQuantity);
    }
}
