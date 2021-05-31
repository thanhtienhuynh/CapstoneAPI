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
    }
}
