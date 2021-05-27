using AutoMapper;
using CapstoneAPI.DataSets.Province;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
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
        public ProvinceService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<Response<IEnumerable<ProvinceDataSet>>> GetAllProvinces()
        {
            Response<IEnumerable<ProvinceDataSet>> response = new Response<IEnumerable<ProvinceDataSet>>();
            IEnumerable<ProvinceDataSet> provinces = (await _uow.ProvinceRepository.Get()).Select(s => _mapper.Map<ProvinceDataSet>(s));
            
                response.Data = provinces;
                response.Message = "Thành công!";
                response.Succeeded = true;
            return response;
        }
    }
}
