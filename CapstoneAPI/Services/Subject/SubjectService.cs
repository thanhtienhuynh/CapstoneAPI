using AutoMapper;
using CapstoneAPI.Controllers;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Major;
using CapstoneAPI.Wrappers;
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
        private readonly ILogger _log = Log.ForContext<SubjectService>();

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
            } catch (Exception ex)
            {
                _log.Error(ex.Message);
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
