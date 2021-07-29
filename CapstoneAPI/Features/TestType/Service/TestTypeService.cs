using AutoMapper;
using CapstoneAPI.Features.TestType.DataSet;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.TestType.Service
{
    public class TestTypeService : ITestTypeService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<TestTypeService>();
        public TestTypeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<Response<IEnumerable<TestTypeDataSet>>> GetAllTestTypes()
        {
            Response<IEnumerable<TestTypeDataSet>> response = new Response<IEnumerable<TestTypeDataSet>>();
            try
            {
                IEnumerable<TestTypeDataSet> testTypes = (await _uow.TestTypeRepository.Get()).
                    Select(s => _mapper.Map<TestTypeDataSet>(s));
                response.Data = testTypes;
                response.Succeeded = true;
            }catch(Exception ex)
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
