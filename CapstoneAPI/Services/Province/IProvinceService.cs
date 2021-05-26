using CapstoneAPI.DataSets.Province;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Province
{
    public interface IProvinceService
    {
        Task<Response<IEnumerable<ProvinceDataSet>>> GetAllProvinces();
    }
}
