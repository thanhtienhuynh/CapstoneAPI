using AutoMapper;
using CapstoneAPI.Features.AdmissionMethod.DataSet;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.AdmissionMethod.Service
{
    public class AdmissitonMethodService : IAdmissionMethodService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<AdmissitonMethodService>();

        public AdmissitonMethodService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<AdmissionMethodDataSet>>> GetAllAdmsstionMethods()
        {
            Response<IEnumerable<AdmissionMethodDataSet>> response = new Response<IEnumerable<AdmissionMethodDataSet>>();
            try
            {
                IEnumerable<AdmissionMethodDataSet> subjects = (await _uow.AdmissionMethodRepository.Get()).Select(s => _mapper.Map<AdmissionMethodDataSet>(s));
                if (!subjects.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không có môn học nào thỏa mãn!");
                }
                else
                {
                    response.Data = subjects;
                    response.Succeeded = true;
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
            return response;
        }
    }
}
