using AutoMapper;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.Major;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
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
                    response.Succeeded = false;
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

        public async Task<Response<IEnumerable<MajorSubjectWeightDataSet>>> GetMajorSubjectWeights(string majorName)
        {
            Response<IEnumerable<MajorSubjectWeightDataSet>> response = null;
            List<MajorSubjectWeightDataSet> majors = (await _uow.MajorRepository
                .Get(filter: m => (string.IsNullOrEmpty(majorName) || m.Name.Contains(majorName))
                && m.Status == Consts.STATUS_ACTIVE)).Select(m => _mapper.Map<MajorSubjectWeightDataSet>(m)).ToList();

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
            response = new Response<IEnumerable<MajorSubjectWeightDataSet>>(majors);
            return response;
        }

        public async Task<PagedResponse<List<MajorToUniversityDataSet>>> GetUniversitiesInMajor(PaginationFilter validFilter, MajorToUniversityFilter majorToUniversityFilter)
        {
            PagedResponse<List<MajorToUniversityDataSet>> response = new PagedResponse<List<MajorToUniversityDataSet>>();
            try
            {
                List<MajorToUniversityDataSet> majorToUniversityDataSets = new List<MajorToUniversityDataSet>();
                Expression<Func<Models.MajorDetail, bool>> filter = null;

                filter = a => (majorToUniversityFilter.Id == null || a.MajorId == majorToUniversityFilter.Id)
                &&(string.IsNullOrWhiteSpace(majorToUniversityFilter.Name) || a.Major.Name.Contains(majorToUniversityFilter.Name))
                && (string.IsNullOrEmpty(majorToUniversityFilter.Code) || a.Major.Code.Contains(majorToUniversityFilter.Code))
                && (a.Status == Consts.STATUS_ACTIVE)
                && (a.Major.Status == Consts.STATUS_ACTIVE)
                && (majorToUniversityFilter.SeasonId == a.SeasonId);
                Func<IQueryable<Models.MajorDetail>, IOrderedQueryable<Models.MajorDetail>> order = null;
                switch (majorToUniversityFilter.Order)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.Major.Code);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.Major.Code);
                        break;
                    case 2:
                        order = order => order.OrderByDescending(a => a.Major.Name);
                        break;
                    case 3:
                        order = order => order.OrderBy(a => a.Major.Name);
                        break;
                }

                IEnumerable<Models.MajorDetail> majorDetails = await _uow.MajorDetailRepository
                .Get(filter: filter, orderBy: order, includeProperties: "Major,University");
                IEnumerable<IGrouping<Models.Major, Models.MajorDetail>> groupbyMajor = majorDetails.GroupBy(m => m.Major);
                foreach (IGrouping<Models.Major, Models.MajorDetail> item in groupbyMajor)
                {
                    MajorToUniversityDataSet majorToUniversityDataSet = new MajorToUniversityDataSet();
                    majorToUniversityDataSet.Id = item.Key.Id;
                    majorToUniversityDataSet.Code = item.Key.Code;
                    majorToUniversityDataSet.Name = item.Key.Name;
                    Dictionary<int, DetailUniversityDataSet> universitiesDictionary = new Dictionary<int, DetailUniversityDataSet>();
                    foreach (Models.MajorDetail majorDetail in item)
                    {
                        if (!universitiesDictionary.ContainsKey(majorDetail.University.Id) && majorDetail.University.Status == Consts.STATUS_ACTIVE)
                        {
                            DetailUniversityDataSet universityBasicDataSet = _mapper.Map<DetailUniversityDataSet>(majorDetail.University);
                            universitiesDictionary.Add(universityBasicDataSet.Id, universityBasicDataSet);
                        }                        
                    }
                    majorToUniversityDataSet.NumberOfUniversity = universitiesDictionary.Count();
                    majorToUniversityDataSets.Add(majorToUniversityDataSet);
                }
                var totalRecords = majorToUniversityDataSets.Count;
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
    }
}
