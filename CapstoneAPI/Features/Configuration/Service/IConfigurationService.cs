using CapstoneAPI.Wrappers;
using System;

namespace CapstoneAPI.Features.Configuration.Service
{
    public interface IConfigurationService
    {
        Response<object> GetPaginationConfiguration();
        Response<object> SetPaginationConfiguration(int? firstPage, int? highestQuantity);
    }
}
