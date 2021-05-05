using AutoMapper;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Major;
using CapstoneAPI.Wrappers;
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
            IEnumerable<SubjectDataSet> subjects = (await _uow.SubjectRepository.Get(filter: s => s.Status == Consts.STATUS_ACTIVE)).Select(s => _mapper.Map<SubjectDataSet>(s));
            if (!subjects.Any())
            {
                response.Succeeded = false;
                response.Errors.Add("Không có môn học nào thỏa mãn!");
            } else
            {
                response.Data = subjects;
                response.Message = "Thành công!";
                response.Succeeded = true;
            }
            return response;
        }
    }
}
