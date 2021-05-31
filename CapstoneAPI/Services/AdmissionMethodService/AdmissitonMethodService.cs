using AutoMapper;
using CapstoneAPI.DataSets.AdmissionMethod;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.AdmissionMethodService
{
    public class AdmissitonMethodService : IAdmissionMethodService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public AdmissitonMethodService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<AdmissionMethodDataSet>>> GetAllAdmsstionMethods()
        {
            Response<IEnumerable<AdmissionMethodDataSet>> response = new Response<IEnumerable<AdmissionMethodDataSet>>();
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
                response.Message = "Thành công!";
                response.Succeeded = true;
            }
            return response;
        }
    }
}
