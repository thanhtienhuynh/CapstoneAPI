using CapstoneAPI.Features.Province.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Province.Service
{
    public interface IProvinceService
    {
        Task<Response<IEnumerable<ProvinceDataSet>>> GetAllProvinces();
    }
}
