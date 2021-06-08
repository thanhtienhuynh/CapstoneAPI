using AutoMapper;
using CapstoneAPI.DataSets.MajorSubjectGroup;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.MajorSubjectGroup
{
    public class MajorSubjectGroupService : IMajorSubjectGroupService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public MajorSubjectGroupService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<MajorSubjectGroupDataSet>>> GetMajorSubjectGourpByMajor(int majorId)
        {
            Response<IEnumerable<MajorSubjectGroupDataSet>> response = new Response<IEnumerable<MajorSubjectGroupDataSet>>();
            IEnumerable<Models.MajorSubjectGroup> majorSubjectGroups = await _uow.MajorSubjectGroupRepository.Get(filter: m => m.MajorId == majorId, includeProperties: "SubjectGroup");
            List<MajorSubjectGroupDataSet> result = new List<MajorSubjectGroupDataSet>();
            if (!majorSubjectGroups.Any())
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Ngành học chưa có khối thi phù hợp!");
                return response;
            }

            foreach (Models.MajorSubjectGroup majorSubjectGroup in majorSubjectGroups)
            {
                MajorSubjectGroupDataSet majorSubjectGroupDataSet = new MajorSubjectGroupDataSet
                {
                    Id = majorSubjectGroup.Id,
                    SubjectGroupName = majorSubjectGroup.SubjectGroup.GroupCode,
                };
                result.Add(majorSubjectGroupDataSet);
            }
            response.Data = result;
            response.Succeeded = true;
            return response;
        }

        public async Task<Response<MajorSubjectGroupDataSet>> AddAMajorSubjectGroup(MajorSubjectGroupParam majorSubjectGroupParam)
        {
            Response<MajorSubjectGroupDataSet> response = new Response<MajorSubjectGroupDataSet>();
            if (majorSubjectGroupParam.MajorId <= 0 || majorSubjectGroupParam.SubjectGroupId <= 0)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Id không hợp lệ!");
                return response;
            }
            Models.MajorSubjectGroup majorSubjectGroup = await _uow.MajorSubjectGroupRepository.GetFirst(filter: m =>
                    m.MajorId == majorSubjectGroupParam.MajorId && m.SubjectGroupId == majorSubjectGroupParam.SubjectGroupId);
            if (majorSubjectGroup != null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Ngành học đã có khối phù hợp!");
                return response;
            }
            Models.Major major = await _uow.MajorRepository.GetById(majorSubjectGroupParam.MajorId);
            Models.SubjectGroup subjectGroup = await _uow.SubjectGroupRepository.GetById(majorSubjectGroupParam.SubjectGroupId);
            if(major == null || subjectGroup == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Khối hoặc ngành không tồn tại!");
                return response;
            }
            Models.MajorSubjectGroup newMajorSubjectGroup = new Models.MajorSubjectGroup
            {
                MajorId = majorSubjectGroupParam.MajorId,
                SubjectGroupId = majorSubjectGroupParam.SubjectGroupId
            };
            _uow.MajorSubjectGroupRepository.Insert(newMajorSubjectGroup);
            if( await _uow.CommitAsync() <= 0)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống!");
                return response;
            }
            MajorSubjectGroupDataSet result = new MajorSubjectGroupDataSet
            {
                Id = newMajorSubjectGroup.Id,
                SubjectGroupName = subjectGroup.GroupCode,
            };
            response.Data = result;
            response.Succeeded = true;



            return response;
        }
    }
}
