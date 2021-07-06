using AutoMapper;
using CapstoneAPI.Features.Article.DataSet;
using CapstoneAPI.Features.Major.DataSet;
using CapstoneAPI.Features.Subject.DataSet;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Features.University.DataSet;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Major;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Major.Service
{
    public class MajorService : IMajorService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly JObject configuration;
        private readonly ILogger _log = Log.ForContext<MajorService>();

        public MajorService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
            string path = Path.Combine(Path
                .GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Configuration\TimeZoneConfiguration.json");
            configuration = JObject.Parse(File.ReadAllText(path));
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
                Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(createMajorDataSet.Code) && m.Status == Consts.STATUS_ACTIVE);
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
                existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(createMajorDataSet.Name) && m.Status == Consts.STATUS_ACTIVE);
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
                Models.Major existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Code.Equals(updateMajor.Code) && m.Status == Consts.STATUS_ACTIVE);
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
                existMajor = await _uow.MajorRepository.GetFirst(filter: m => m.Name.Equals(updateMajor.Name) && m.Status == Consts.STATUS_ACTIVE);
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
                objToUpdate.Description = updateMajor.Description;
                objToUpdate.Curriculum = updateMajor.Curriculum;
                objToUpdate.HumanQuality = updateMajor.HumanQuality;
                objToUpdate.SalaryDescription = updateMajor.SalaryDescription;
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
                            (await _uow.MajorSubjectGroupRepository.Get(filter: ms => ms.MajorId == item.Id && ms.Status == Consts.STATUS_ACTIVE,
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
                            (await _uow.MajorSubjectGroupRepository.Get(filter: ms => ms.MajorId == item.Id && ms.Status == Consts.STATUS_ACTIVE,
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
                                        int id = 0;
                                        int weight = subjectWeight.Weight;
                                        string name = "";
                                        bool isSpecialSubjectGroup = false;

                                        if (subjectWeight.SubjectGroupDetail.Subject != null)
                                        {
                                            id = subjectWeight.SubjectGroupDetail.Subject.Id;
                                            name = subjectWeight.SubjectGroupDetail.Subject.Name;
                                        }
                                        else if (subjectWeight.SubjectGroupDetail.SpecialSubjectGroup != null)
                                        {
                                            id = subjectWeight.SubjectGroupDetail.SpecialSubjectGroup.Id;
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
                            SubjectGroupId = subjectGroup.Id,
                            Status = Consts.STATUS_ACTIVE
                        };
                        //Có thể có lỗi ko đúng SubjectGroupId
                        _uow.MajorSubjectGroupRepository.Insert(majorSubjectGroup);

                        List<CreateMajorSubjectWeight> subjectWeights = subjectGroup.SubjectWeights;
                        List<Models.SubjectWeight> newSubjectWeights = new List<Models.SubjectWeight>();

                        foreach (var subjectWeight in subjectWeights)
                        {
                            var subjectGroupDetail = await _uow.SubjecGroupDetailRepository
                                .GetFirst(filter: s => (s.SubjectGroupId == subjectGroup.Id)
                                && (subjectWeight.IsSpecialSubjectGroup ? s.SpecialSubjectGroupId == subjectWeight.SubjectId
                                : s.SubjectId == subjectWeight.SubjectId));

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

        public async Task<Response<UpdateMajorParam>> UpdateMajor(UpdateMajorParam updateMajor)
        {
            Response<UpdateMajorParam> response = new Response<UpdateMajorParam>();

            try
            {
                if (updateMajor.Name.Trim().Equals("") || updateMajor.Code.Trim().Equals("") ||
                    (updateMajor.Status != Consts.STATUS_ACTIVE && updateMajor.Status != Consts.STATUS_INACTIVE))
                {
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


                if (updateMajor.SubjectGroup != null)
                {
                    Models.MajorSubjectGroup marjorSubjectGroup = await _uow.MajorSubjectGroupRepository
                        .GetFirst(filter: m => m.MajorId == updateMajor.Id 
                        && m.SubjectGroupId == updateMajor.SubjectGroup.Id
                        && m.Status == Consts.STATUS_ACTIVE);

                    if (marjorSubjectGroup != null)
                    {
                        foreach (var subjectWeight in updateMajor.SubjectGroup.SubjectWeights)
                        {
                            Models.SubjectGroupDetail subjectGroupDetail = await _uow.SubjecGroupDetailRepository
                                .GetFirst(s => (s.SubjectGroupId == updateMajor.SubjectGroup.Id) && (subjectWeight.IsSpecialSubjectGroup
                                ? s.SpecialSubjectGroupId == subjectWeight.SubjectId : s.SubjectId == subjectWeight.SubjectId));

                            if (subjectGroupDetail == null)
                            {
                                if (response.Errors == null)
                                {
                                    response.Errors = new List<string>();
                                }
                                response.Errors.Add("Không thể cập nhật ngành!");
                                return response;
                            }

                            int subjectGroupDetailId = subjectGroupDetail.Id;

                            Models.SubjectWeight updateSubjectWeight = await _uow.SubjectWeightRepository
                                .GetFirst(s => s.MajorSubjectGroupId == marjorSubjectGroup.Id && s.SubjectGroupDetailId == subjectGroupDetailId);

                            if (updateSubjectWeight == null)
                            {
                                if (response.Errors == null)
                                {
                                    response.Errors = new List<string>();
                                }
                                response.Errors.Add("Không thể cập nhật ngành!");
                                return response;
                            }

                            updateSubjectWeight.Weight = subjectWeight.Weight;
                            _uow.SubjectWeightRepository.Update(updateSubjectWeight);
                        }

                    }
                    else
                    {
                        Models.MajorSubjectGroup majorSubjectGroup = new Models.MajorSubjectGroup()
                        {
                            MajorId = updateMajor.Id,
                            SubjectGroupId = updateMajor.SubjectGroup.Id,
                            Status = Consts.STATUS_ACTIVE
                        };

                        _uow.MajorSubjectGroupRepository.Insert(majorSubjectGroup);

                        List<CreateMajorSubjectWeight> subjectWeights = updateMajor.SubjectGroup.SubjectWeights;
                        List<Models.SubjectWeight> newSubjectWeights = new List<Models.SubjectWeight>();

                        foreach (var subjectWeight in subjectWeights)
                        {
                            var subjectGroupDetail = await _uow.SubjecGroupDetailRepository
                                .GetFirst(filter: s => (s.SubjectGroupId == updateMajor.SubjectGroup.Id)
                                && (subjectWeight.IsSpecialSubjectGroup ? s.SpecialSubjectGroupId == subjectWeight.SubjectId
                                : s.SubjectId == subjectWeight.SubjectId));

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
                    response.Errors.Add("Không thể cập nhật ngành!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = updateMajor;
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

        public async Task<Response<UpdateMajorParam2>> UpdateMajor(UpdateMajorParam2 updateMajor)
        {
            Response<UpdateMajorParam2> response = new Response<UpdateMajorParam2>();

            try
            {
                if (updateMajor.Name.Trim().Equals("") || updateMajor.Code.Trim().Equals("") ||
                    (updateMajor.Status != Consts.STATUS_ACTIVE && updateMajor.Status != Consts.STATUS_INACTIVE))
                {
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
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Ngành này không tồn tại trong hệ thống!");
                    return response;
                }
                objToUpdate.Code = updateMajor.Code;
                objToUpdate.Name = updateMajor.Name;
                objToUpdate.Description = updateMajor.Description;
                objToUpdate.Curriculum = updateMajor.Curriculum;
                objToUpdate.HumanQuality = updateMajor.HumanQuality;
                objToUpdate.SalaryDescription = updateMajor.SalaryDescription;
                objToUpdate.Status = updateMajor.Status;
                _uow.MajorRepository.Update(objToUpdate);


                if (updateMajor.SubjectGroup != null && updateMajor.SubjectGroup.Count() > 0)
                {
                    foreach (var item in updateMajor.SubjectGroup)
                    {
                        Models.MajorSubjectGroup majorSubjectGroup = await _uow.MajorSubjectGroupRepository
                            .GetFirst(filter: m => m.MajorId == updateMajor.Id && m.SubjectGroupId == item.Id && m.Status == Consts.STATUS_ACTIVE);

                        if (majorSubjectGroup != null)
                        {
                            if (item.Status == Consts.STATUS_ACTIVE)
                            {
                                foreach (var subjectWeight in item.SubjectWeights)
                                {
                                    Models.SubjectGroupDetail subjectGroupDetail = await _uow.SubjecGroupDetailRepository
                                        .GetFirst(s => (s.SubjectGroupId == item.Id) && (subjectWeight.IsSpecialSubjectGroup
                                        ? s.SpecialSubjectGroupId == subjectWeight.SubjectId : s.SubjectId == subjectWeight.SubjectId));

                                    if (subjectGroupDetail == null)
                                    {
                                        if (response.Errors == null)
                                        {
                                            response.Errors = new List<string>();
                                        }
                                        response.Errors.Add("Không thể cập nhật ngành!");
                                        return response;
                                    }

                                    int subjectGroupDetailId = subjectGroupDetail.Id;

                                    Models.SubjectWeight updateSubjectWeight = await _uow.SubjectWeightRepository
                                        .GetFirst(s => s.MajorSubjectGroupId == majorSubjectGroup.Id && s.SubjectGroupDetailId == subjectGroupDetailId);

                                    if (updateSubjectWeight == null)
                                    {
                                        if (response.Errors == null)
                                        {
                                            response.Errors = new List<string>();
                                        }
                                        response.Errors.Add("Không thể cập nhật ngành!");
                                        return response;
                                    }

                                    updateSubjectWeight.Weight = subjectWeight.Weight;
                                    _uow.SubjectWeightRepository.Update(updateSubjectWeight);
                                }
                            }
                            else
                            {
                                majorSubjectGroup.Status = item.Status;
                                _uow.MajorSubjectGroupRepository.Update(majorSubjectGroup);
                                Dictionary<int, Models.User> users = new Dictionary<int, Models.User>();
                                IEnumerable<Models.EntryMark> entryMarks = await _uow
                                    .EntryMarkRepository.Get(filter: e => e.MajorSubjectGroupId == majorSubjectGroup.Id && e.Status == Consts.STATUS_ACTIVE
                                    , includeProperties: "FollowingDetails");
                                if (entryMarks.Any())
                                {
                                    foreach (var entryMark in entryMarks)
                                    {
                                        entryMark.Status = item.Status;
                                        if (entryMark.FollowingDetails.Where(w => w.Status == Consts.STATUS_ACTIVE).Any())
                                        {
                                            foreach (var followingDetail in entryMark.FollowingDetails.Where(w => w.Status == Consts.STATUS_ACTIVE))
                                            {
                                                followingDetail.Status = item.Status;
                                                if (!users.ContainsKey(followingDetail.UserId))
                                                {
                                                    users.Add(followingDetail.UserId, followingDetail.User);
                                                }
                                            }
                                            _uow.FollowingDetailRepository.UpdateRange(entryMark.FollowingDetails);
                                        }
                                    }
                                    _uow.EntryMarkRepository.UpdateRange(entryMarks);
                                }
                            }
                        }
                        else
                        {
                            Models.MajorSubjectGroup newMajorSubjectGroup = new Models.MajorSubjectGroup()
                            {
                                MajorId = updateMajor.Id,
                                SubjectGroupId = item.Id,
                                Status = Consts.STATUS_ACTIVE
                            };

                            _uow.MajorSubjectGroupRepository.Insert(newMajorSubjectGroup);

                            List<CreateMajorSubjectWeight> subjectWeights = item.SubjectWeights;
                            List<Models.SubjectWeight> newSubjectWeights = new List<Models.SubjectWeight>();

                            foreach (var subjectWeight in subjectWeights)
                            {
                                var subjectGroupDetail = await _uow.SubjecGroupDetailRepository
                                    .GetFirst(filter: s => (s.SubjectGroupId == item.Id)
                                    && (subjectWeight.IsSpecialSubjectGroup ? s.SpecialSubjectGroupId == subjectWeight.SubjectId
                                    : s.SubjectId == subjectWeight.SubjectId));

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
                                    MajorSubjectGroup = newMajorSubjectGroup
                                });
                            }

                            _uow.SubjectWeightRepository.InsertRange(newSubjectWeights);
                        }
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
                    response.Errors.Add("Không thể cập nhật ngành!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = updateMajor;
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

        public async Task<PagedResponse<List<NumberUniversityInMajorDataSet>>> GetNumberUniversitiesInMajor(PaginationFilter validFilter, MajorToNumberUniversityFilter majorToNumberUniversityFilter)
        {
            PagedResponse<List<NumberUniversityInMajorDataSet>> response = new PagedResponse<List<NumberUniversityInMajorDataSet>>();
            try
            {
                Models.Season season = await _uow.SeasonRepository.GetCurrentSeason();
                List<NumberUniversityInMajorDataSet> majorToUniversityDataSets = new List<NumberUniversityInMajorDataSet>();
                Expression<Func<Models.Major, bool>> filter = null;
                filter = a => (string.IsNullOrWhiteSpace(majorToNumberUniversityFilter.Name) || a.Name.Contains(majorToNumberUniversityFilter.Name))
                && (string.IsNullOrEmpty(majorToNumberUniversityFilter.Code) || a.Code.Contains(majorToNumberUniversityFilter.Code))
                && (a.Status == Consts.STATUS_ACTIVE);
                Func<IQueryable<Models.Major>, IOrderedQueryable<Models.Major>> order = null;
                switch (majorToNumberUniversityFilter.Order)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.Code);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.Code);
                        break;
                    case 2:
                        order = order => order.OrderByDescending(a => a.Name);
                        break;
                    case 3:
                        order = order => order.OrderBy(a => a.Name);
                        break;
                }

                IEnumerable<Models.Major> majors = await _uow.MajorRepository
                .Get(filter: filter, orderBy: order);


                majorToUniversityDataSets = majors.Select(s => _mapper.Map<NumberUniversityInMajorDataSet>(s)).ToList();
                if (season != null)
                {
                    foreach (var item in majorToUniversityDataSets)
                    {

                        item.NumberOfUniversity = (await _uow.MajorDetailRepository.Get(filter: m => m.MajorId == item.Id
                                                  && m.Status == Consts.STATUS_ACTIVE && m.SeasonId == season.Id))
                                                  .Select(s => s.UniversityId).Distinct().Count();
                    }
                }
                majorToUniversityDataSets = majorToUniversityDataSets.OrderByDescending(m => m.NumberOfUniversity)
                    .Skip((validFilter.PageNumber - 1) * validFilter.PageSize).Take(validFilter.PageSize).ToList();


                var totalRecords = majors.Count();
                if (totalRecords <= 0)
                {
                    response.Succeeded = true;
                    response.Message = "Không tìm thấy ngành nào để hiển thị!";
                }
                else
                {
                    response = PaginationHelper.CreatePagedReponse(majorToUniversityDataSets, validFilter, totalRecords);
                }
            }
            catch (Exception ex)
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

        public async Task<Response<MajorDetailDataSet>> GetUniversitiesInMajor(int majorId)
        {
            Response<MajorDetailDataSet> response = new Response<MajorDetailDataSet>();
            try
            {
                Models.Season season = await _uow.SeasonRepository.GetCurrentSeason();
                Models.Major major = await _uow.MajorRepository.GetFirst(filter: m => m.Id == majorId && m.Status == Consts.STATUS_ACTIVE,
                                    includeProperties: "MajorArticles.Article,MajorCareers.Career," +
                                    "MajorDetails.University");
                if (major == null)
                {
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Ngành này không tồn tại!");
                    return response;
                }
                List<DetailUniversityDataSet> majorToUniversityDataSet = new List<DetailUniversityDataSet>();
                List<ArticleDetailDataSet> articleDataSets = new List<ArticleDetailDataSet>();
                List<CareerDataSet> careerDataSets = new List<CareerDataSet>();
                List<MajorDetailSubjectGroupDataSet> subjectGroupDataSets = new List<MajorDetailSubjectGroupDataSet>();

                MajorDetailDataSet result = _mapper.Map<MajorDetailDataSet>(major);

                if (season != null && major.MajorDetails != null
                    && major.MajorDetails.Where(m => m.Status == Consts.STATUS_ACTIVE && m.SeasonId == season.Id).Any()) {
                    IEnumerable<Models.University> universities = major.MajorDetails.Where(m => m.Status == Consts.STATUS_ACTIVE && m.SeasonId == season.Id)
                                                                    .Select(m => m.University).Distinct();
                    foreach (Models.University university in universities)
                    {
                        majorToUniversityDataSet.Add(_mapper.Map<DetailUniversityDataSet>(university));
                    }
                }

                var currentTimeZone = configuration.SelectToken("CurrentTimeZone").ToString();
                DateTime currentDate = DateTime.UtcNow.AddHours(int.Parse(currentTimeZone));

                if (major.MajorArticles != null && major.MajorArticles.Where(m => m.Article.Status == 3
                && m.Article.PublicFromDate != null && m.Article.PublicToDate != null && DateTime.Compare((DateTime)m.Article.PublicToDate, currentDate) > 0).Any())
                {
                    IEnumerable<Models.Article> articles = major.MajorArticles.Where(m => m.Article.Status == 3
                                        && m.Article.PublicFromDate != null && m.Article.PublicToDate != null
                                        && DateTime.Compare((DateTime)m.Article.PublicToDate, currentDate) > 0)
                                        .Select(m => m.Article);
                    foreach (Models.Article article in articles)
                    {
                        articleDataSets.Add(_mapper.Map<ArticleDetailDataSet>(article));
                    }
                }

                if (major.MajorCareers != null && major.MajorCareers.Where(m => m.Career.Status == Consts.STATUS_ACTIVE).Any())
                {
                    IEnumerable<Career> careers = major.MajorCareers.Where(m => m.Career.Status == Consts.STATUS_ACTIVE)
                                            .Select(m => m.Career);
                    foreach (Career career in careers)
                    {
                        careerDataSets.Add(_mapper.Map<CareerDataSet>(career));
                    }
                }

                IEnumerable<Models.MajorSubjectGroup> majorSubjectGroups = await _uow.MajorSubjectGroupRepository.Get(
                            filter: m => m.SubjectGroup.Status == Consts.STATUS_ACTIVE && m.MajorId == major.Id,
                            includeProperties: "SubjectGroup.SubjectGroupDetails.Subject,SubjectGroup.SubjectGroupDetails.SpecialSubjectGroup");
                if (majorSubjectGroups.Any())
                {
                    foreach (Models.MajorSubjectGroup majorSubjectGroup in majorSubjectGroups)
                    {
                        MajorDetailSubjectGroupDataSet subjectGroup = new MajorDetailSubjectGroupDataSet();
                        List<string> subjects = new List<string>();
                        foreach (SubjectGroupDetail subjectGroupDetail in majorSubjectGroup.SubjectGroup.SubjectGroupDetails)
                        {
                            if (subjectGroupDetail.Subject != null)
                            {
                                subjects.Add(subjectGroupDetail.Subject.Name);
                            }
                            else if (subjectGroupDetail.SpecialSubjectGroup != null)
                            {
                                subjects.Add(subjectGroupDetail.SpecialSubjectGroup.Name);
                            }
                        }
                        subjectGroup.Subjects = subjects;
                        subjectGroup.Name = majorSubjectGroup.SubjectGroup.GroupCode;
                        subjectGroup.Id = majorSubjectGroup.SubjectGroup.Id;
                        subjectGroupDataSets.Add(subjectGroup);
                    }
                }

                result.Universities = majorToUniversityDataSet;
                result.Articles = articleDataSets;
                result.Careers = careerDataSets;
                result.SubjectGroups = subjectGroupDataSets.OrderBy(s => s.Name).ToList();
                response.Data = result;
                response.Succeeded = true;
                return response;
            }
            catch (Exception ex)
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
