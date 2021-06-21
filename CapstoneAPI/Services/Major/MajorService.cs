using AutoMapper;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.Filters;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Major
{
    public class MajorService : IMajorService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<MajorService>();

        public MajorService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<Response<IEnumerable<AdminMajorDataSet>>> GetActiveMajorsByAdmin()
        {
            Response<IEnumerable<AdminMajorDataSet>> response = new Response<IEnumerable<AdminMajorDataSet>>();
            try
            {
                IEnumerable<AdminMajorDataSet> adminMajorDataSets = (await _uow.MajorRepository.Get(filter: m => m.Status == Consts.STATUS_ACTIVE))
                .Select(m => _mapper.Map<AdminMajorDataSet>(m));
                if (adminMajorDataSets == null || !adminMajorDataSets.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không có ngành học thỏa mãn!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = adminMajorDataSets;
                }
            }
            catch (Exception ex)
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

        public async Task<Response<ResultOfCreateMajorDataSet>> CreateAMajor(CreateMajorDataSet createMajorDataSet)
        {
            Response<ResultOfCreateMajorDataSet> response = new Response<ResultOfCreateMajorDataSet>();
            try
            {
                if (createMajorDataSet.Name.Equals("") || createMajorDataSet.Code.Trim().Equals(""))
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành không được để trống!");
                    return response;
                }
                Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(createMajorDataSet.Code));
                if (existMajor != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành đã tồn tại!");
                    return response;
                }
                existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(createMajorDataSet.Name));
                if (existMajor != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên ngành đã tồn tại!");
                    return response;
                }
                Models.Major newMajor = _mapper.Map<Models.Major>(createMajorDataSet);
                newMajor.Status = Consts.STATUS_ACTIVE;
                _uow.MajorRepository.Insert(newMajor);
                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên ngành đã tồn tại!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = _mapper.Map<ResultOfCreateMajorDataSet>(newMajor);
                }
            }
            catch (Exception ex)
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

        public async Task<Response<ResultOfCreateMajorDataSet>> UpdateAMajor(ResultOfCreateMajorDataSet updateMajor)
        {
            Response<ResultOfCreateMajorDataSet> response = new Response<ResultOfCreateMajorDataSet>();
            try
            {
                if (updateMajor.Name.Trim().Equals("") || updateMajor.Code.Trim().Equals("") || (updateMajor.Status != Consts.STATUS_ACTIVE && updateMajor.Status != Consts.STATUS_INACTIVE))
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Dữ liệu bị thiếu!");
                    return response;
                }
                Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(updateMajor.Code));
                if (existMajor != null && existMajor.Id != updateMajor.Id)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành cập nhật đã tồn tại!");
                    return response;
                }
                existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(updateMajor.Name));
                if (existMajor != null && existMajor.Id != updateMajor.Id)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên ngành cập nhật đã tồn tại!");
                    return response;
                }
                Models.Major objToUpdate = await _uow.MajorRepository.GetFirst(filter: m => m.Id.Equals(updateMajor.Id));
                if (objToUpdate == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Ngành này không tồn tại trong hệ thống!");
                    return response;
                }
                objToUpdate.Code = updateMajor.Code;
                objToUpdate.Name = updateMajor.Name;
                objToUpdate.Status = updateMajor.Status;
                _uow.MajorRepository.Update(objToUpdate);
                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Ngành này không tồn tại trong hệ thống!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = _mapper.Map<ResultOfCreateMajorDataSet>(objToUpdate);
                }
            }
            catch (Exception ex)
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

        public async Task<Response<List<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(string majorName)
        {
            Response<List<MajorSubjectWeightDataSet>> response = new Response<List<MajorSubjectWeightDataSet>>();
            try
            {
                List<MajorSubjectWeightDataSet> majors = (await _uow.MajorRepository
                    .Get(filter: m => (string.IsNullOrEmpty(majorName) || m.Name.Contains(majorName))
                    && m.Status == Consts.STATUS_ACTIVE, orderBy: o => o.OrderBy(m => m.Name)))
                    .Select(m => _mapper.Map<MajorSubjectWeightDataSet>(m)).ToList();
                if (majors != null && majors.Count > 0)
                {

                    foreach (var item in majors)
                    {
                        List<Models.MajorSubjectGroup> majorSubjectGroups =
                            (await _uow.MajorSubjectGroupRepository.Get(filter: ms => ms.MajorId == item.Id,
                            includeProperties: "SubjectGroup,SubjectWeights,SubjectWeights.SubjectGroupDetail," +
                            "SubjectWeights.SubjectGroupDetail.Subject,SubjectWeights.SubjectGroupDetail.SpecialSubjectGroup")).ToList();

                        if (majorSubjectGroups != null && majorSubjectGroups.Count() > 0)
                        {

                            List<SubjectGroupWeightDataSet> subjectGroupWeightDataSets = new List<SubjectGroupWeightDataSet>();

                            foreach (var majorSubjectGroup in majorSubjectGroups)
                            {
                                int subjectGroupWeightDataSetId = majorSubjectGroup.SubjectGroup.Id;
                                string subjectGroupWeightDataSetGroupCode = majorSubjectGroup.SubjectGroup.GroupCode;

                                if (majorSubjectGroup.SubjectWeights != null && majorSubjectGroup.SubjectWeights.Count > 0)
                                {
                                    List<SubjectWeightDataSet> subjectWeightDataSets = new List<SubjectWeightDataSet>();
                                    SubjectWeightDataSet subjectWeightDataSet = new SubjectWeightDataSet();

                                    foreach (var subjectWeight in majorSubjectGroup.SubjectWeights)
                                    {
                                        int id = subjectWeight.Id;
                                        int weight = subjectWeight.Weight;
                                        string name = "";
                                        bool isSpecialSubjectGroup = false;

                                        if (subjectWeight.SubjectGroupDetail.Subject != null)
                                            name = subjectWeight.SubjectGroupDetail.Subject.Name;
                                        else if (subjectWeight.SubjectGroupDetail.SpecialSubjectGroup != null)
                                        {
                                            name = subjectWeight.SubjectGroupDetail.SpecialSubjectGroup.Name;
                                            isSpecialSubjectGroup = true;
                                        }

                                        subjectWeightDataSets.Add(new SubjectWeightDataSet()
                                        {
                                            Id = id,
                                            Weight = weight,
                                            Name = name,
                                            IsSpecialSubjectGroup = isSpecialSubjectGroup
                                        });
                                    }
                                    subjectGroupWeightDataSets.Add(new SubjectGroupWeightDataSet()
                                    {
                                        Id = subjectGroupWeightDataSetId,
                                        GroupCode = subjectGroupWeightDataSetGroupCode,
                                        SubjectWeights = subjectWeightDataSets
                                    });
                                }
                            }
                            item.SubjectGroups = subjectGroupWeightDataSets;
                        }
                    }
                    response.Succeeded = true;
                    response.Data = majors;
                }
                else
                {
                    response.Succeeded = true;
                    response.Message = "Không tìm được ngành học phù hợp!";
                }

            }
            catch (Exception ex)
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

        public async Task<PagedResponse<List<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(PaginationFilter validFilter, string majorName)
        {
            PagedResponse<List<MajorSubjectWeightDataSet>> response = new PagedResponse<List<MajorSubjectWeightDataSet>>();

            try
            {

                Expression<Func<Models.Major, bool>> filter = null;
                filter = m => (string.IsNullOrEmpty(majorName) || m.Name.Contains(majorName))
                    && m.Status == Consts.STATUS_ACTIVE;


                List<MajorSubjectWeightDataSet> majors = (await _uow.MajorRepository
                    .Get(filter: filter, first: validFilter.PageSize,
                    offset: (validFilter.PageNumber - 1) * validFilter.PageSize, orderBy: o => o.OrderBy(m => m.Name)))
                    .Select(m => _mapper.Map<MajorSubjectWeightDataSet>(m)).ToList();

                var totalRecords = _uow.MajorRepository.Count(filter: filter);
                if (majors != null && majors.Count > 0)
                {

                    foreach (var item in majors)
                    {
                        List<Models.MajorSubjectGroup> majorSubjectGroups =
                            (await _uow.MajorSubjectGroupRepository.Get(filter: ms => ms.MajorId == item.Id,
                            includeProperties: "SubjectGroup,SubjectWeights,SubjectWeights.SubjectGroupDetail," +
                            "SubjectWeights.SubjectGroupDetail.Subject,SubjectWeights.SubjectGroupDetail.SpecialSubjectGroup")).ToList();

                        if (majorSubjectGroups != null && majorSubjectGroups.Count() > 0)
                        {

                            List<SubjectGroupWeightDataSet> subjectGroupWeightDataSets = new List<SubjectGroupWeightDataSet>();

                            foreach (var majorSubjectGroup in majorSubjectGroups)
                            {
                                int subjectGroupWeightDataSetId = majorSubjectGroup.SubjectGroup.Id;
                                string subjectGroupWeightDataSetGroupCode = majorSubjectGroup.SubjectGroup.GroupCode;

                                if (majorSubjectGroup.SubjectWeights != null && majorSubjectGroup.SubjectWeights.Count > 0)
                                {
                                    List<SubjectWeightDataSet> subjectWeightDataSets = new List<SubjectWeightDataSet>();
                                    SubjectWeightDataSet subjectWeightDataSet = new SubjectWeightDataSet();

                                    foreach (var subjectWeight in majorSubjectGroup.SubjectWeights)
                                    {
                                        int id = subjectWeight.Id;
                                        int weight = subjectWeight.Weight;
                                        string name = "";
                                        bool isSpecialSubjectGroup = false;

                                        if (subjectWeight.SubjectGroupDetail.Subject != null)
                                            name = subjectWeight.SubjectGroupDetail.Subject.Name;
                                        else if (subjectWeight.SubjectGroupDetail.SpecialSubjectGroup != null)
                                        {
                                            name = subjectWeight.SubjectGroupDetail.SpecialSubjectGroup.Name;
                                            isSpecialSubjectGroup = true;
                                        }

                                        subjectWeightDataSets.Add(new SubjectWeightDataSet()
                                        {
                                            Id = id,
                                            Weight = weight,
                                            Name = name,
                                            IsSpecialSubjectGroup = isSpecialSubjectGroup
                                        });
                                    }
                                    subjectGroupWeightDataSets.Add(new SubjectGroupWeightDataSet()
                                    {
                                        Id = subjectGroupWeightDataSetId,
                                        GroupCode = subjectGroupWeightDataSetGroupCode,
                                        SubjectWeights = subjectWeightDataSets
                                    });
                                }
                            }
                            item.SubjectGroups = subjectGroupWeightDataSets;
                        }
                    }
                    response = PaginationHelper.CreatePagedReponse(majors, validFilter, totalRecords);
                }
                else
                {
                    response.Succeeded = true;
                    response.Message = "Không tìm được ngành học phù hợp!";
                }

            }
            catch (Exception ex)
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

        public async Task<Response<CreateMajorSubjectWeightDataSet>> CreateAMajor(CreateMajorSubjectWeightDataSet createMajor)
        {
            Response<CreateMajorSubjectWeightDataSet> response = new Response<CreateMajorSubjectWeightDataSet>();
            try
            {
                if (createMajor.Name.Equals("") || createMajor.Code.Trim().Equals(""))
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành không được để trống!");
                    return response;
                }
                Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(createMajor.Code));
                if (existMajor != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mã ngành đã tồn tại!");
                    return response;
                }
                existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(createMajor.Name));
                if (existMajor != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên ngành đã tồn tại!");
                    return response;
                }
                Models.Major newMajor = _mapper.Map<Models.Major>(createMajor);
                newMajor.Status = Consts.STATUS_ACTIVE;
                _uow.MajorRepository.Insert(newMajor);

                if (createMajor.SubjectGroups != null && createMajor.SubjectGroups.Count() > 0)
                {
                    foreach (var subjectGroup in createMajor.SubjectGroups)
                    {
                        Models.MajorSubjectGroup majorSubjectGroup = new Models.MajorSubjectGroup()
                        {
                            Major = newMajor,
                            SubjectGroupId = subjectGroup.Id
                        };
                        //Có thể có lỗi ko đúng SubjectGroupId
                        _uow.MajorSubjectGroupRepository.Insert(majorSubjectGroup);

                        List<SubjectWeightDataSet> subjectWeights = subjectGroup.SubjectWeights;
                        List<Models.SubjectWeight> newSubjectWeights = new List<Models.SubjectWeight>();

                        foreach (var subjectWeight in subjectWeights)
                        {
                            var subjectGroupDetail = await _uow.SubjecGroupDetailRepository
                                .GetFirst(filter: s => (s.SubjectGroupId == subjectGroup.Id)
                                && (subjectWeight.IsSpecialSubjectGroup ? s.SpecialSubjectGroupId == subjectWeight.Id
                                : s.SubjectId == subjectWeight.Id));

                            if (subjectGroupDetail == null)
                            {
                                response.Succeeded = false;
                                if (response.Errors == null)
                                {
                                    response.Errors = new List<string>();
                                }
                                response.Errors.Add("Môn học hoặc tổ hợp không phù hợp!");
                                return response;
                            }
                            int subjectGroupDetailId = subjectGroupDetail.Id;
                            newSubjectWeights.Add(new Models.SubjectWeight()
                            {
                                SubjectGroupDetailId = subjectGroupDetailId,
                                Weight = subjectWeight.Weight,
                                MajorSubjectGroup = majorSubjectGroup
                            });
                        }

                        _uow.SubjectWeightRepository.InsertRange(newSubjectWeights);
                    }
                }

                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Hiện tại Không thể thêm ngành!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = createMajor;
                }

            }
            catch (Exception ex)
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
