using AutoMapper;
using CapstoneAPI.DataSets.Province;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Province
{
    public class ProvinceService : IProvinceService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<ProvinceService>();

        public ProvinceService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<Response<IEnumerable<ProvinceDataSet>>> GetAllProvinces()
        {
            Response<IEnumerable<ProvinceDataSet>> response = new Response<IEnumerable<ProvinceDataSet>>();
            try
            {
                IEnumerable<ProvinceDataSet> provinces = (await _uow.ProvinceRepository.Get()).Select(s => _mapper.Map<ProvinceDataSet>(s));

                response.Data = provinces;
                response.Succeeded = true;
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
    }
}
