using AutoMapper;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Major;
using CapstoneAPI.Wrappers;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Subject
{
    public class SubjectService : ISubjectService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public SubjectService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<SubjectDataSet>>> GetAllSubjects()
        {
            Response<IEnumerable<SubjectDataSet>> response = new Response<IEnumerable<SubjectDataSet>>();
            try
            {
                IEnumerable<SubjectDataSet> subjects = (await _uow.SubjectRepository.Get(filter: s => s.Status == Consts.STATUS_ACTIVE)).Select(s => _mapper.Map<SubjectDataSet>(s));
                response.Data = subjects;
                response.Succeeded = true;
                throw new Exception("Test lỗi");
            } catch (Exception ex)
            {
                Log.Error(ex.Message);
                response.Succeeded = true;
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
