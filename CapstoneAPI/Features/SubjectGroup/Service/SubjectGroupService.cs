using AutoMapper;
using CapstoneAPI.DataSets.SpecialSubjectGroup;
using CapstoneAPI.Features.Major.DataSet;
using CapstoneAPI.Features.Subject.DataSet;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.SubjectGroup.Service
{
    public class SubjectGroupService : ISubjectGroupService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<SubjectGroupService>();

        public SubjectGroupService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<SubjectGroupDataSet>>> GetCaculatedSubjectGroup(SubjectGroupParam subjectGroupParam)
        {
            Response<IEnumerable<SubjectGroupDataSet>> response = new Response<IEnumerable<SubjectGroupDataSet>>();
            try
            {
                List<SubjectGroupDataSet> subjectGroupDataSets = new List<SubjectGroupDataSet>();
                //Lấy danh sách khối
                IEnumerable<Models.SubjectGroup> subjectGroups = await _uow.SubjectGroupRepository
                    .Get(filter: s => s.Status == Consts.STATUS_ACTIVE, includeProperties: "SubjectGroupDetails");

                if (!subjectGroups.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Hệ thống ghi nhận không có tổ hợp môn nào!");
                    return response;
                }

                //Tính điểm mỗi khối
                foreach (Models.SubjectGroup subjectGroup in subjectGroups)
                {
                    double totalMark = await CalculateSubjectGroupMark(subjectGroupParam.Marks, subjectGroup.SubjectGroupDetails.ToList());
                    if (totalMark > 0)
                    {
                        subjectGroupDataSets.Add(new SubjectGroupDataSet
                        {
                            TotalMark = totalMark,
                            Name = subjectGroup.GroupCode,
                            Id = subjectGroup.Id
                        });
                    }
                }

                if (!subjectGroupDataSets.Any())
                {
                    response.Succeeded = true;
                    response.Data = subjectGroupDataSets;
                    return response;
                }

                Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
                Models.Season previousSeason = await _uow.SeasonRepository.GetPreviousSeason();

                if (currentSeason == null || previousSeason == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mùa tuyển sinh chưa được kích hoạt!");
                    return response;
                }

                List<SubjectGroupDataSet> suggestedSubjectGroups = new List<SubjectGroupDataSet>();

                foreach (SubjectGroupDataSet subjectGroupDataSet in subjectGroupDataSets)
                {
                    IEnumerable<Models.Major> majors = (await _uow.MajorSubjectGroupRepository
                        .Get(filter: s => s.SubjectGroupId == subjectGroupDataSet.Id && s.Major.Status == Consts.STATUS_ACTIVE,
                            includeProperties: "Major")).Select(s => s.Major);
                    List<MajorDataSet> majorDataSets = new List<MajorDataSet>();
                    foreach (Models.Major major in majors)
                    {
                        var groupsByMajorDetails = (await _uow.MajorDetailRepository
                            .Get(filter: m => m.MajorId == major.Id && m.Status == Consts.STATUS_ACTIVE,
                                includeProperties: "AdmissionCriterion,AdmissionCriterion.SubAdmissionCriteria"))
                            .GroupBy(m => new { m.UniversityId, m.TrainingProgramId });

                        bool isValid = false;
                        double highestEntryMark = 0;
                        foreach (var groupsByMajorDetail in groupsByMajorDetails)
                        {
                            MajorDetail currentMajorDetail = groupsByMajorDetail.Where(m => m.SeasonId == currentSeason.Id).FirstOrDefault();
                            MajorDetail previousMajorDetail = groupsByMajorDetail.Where(m => m.SeasonId == previousSeason.Id).FirstOrDefault();
                            if (currentMajorDetail == null || previousMajorDetail == null)
                            {
                                continue;
                            }

                            if (currentMajorDetail.AdmissionCriterion == null || previousMajorDetail.AdmissionCriterion == null)
                            {
                                continue;
                            }

                            if (currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria == null
                                || !currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.AdmissionMethodId == 1 && s.Status == Consts.STATUS_ACTIVE).Any()
                                || previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria == null
                                || !previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.AdmissionMethodId == 1 && s.Status == Consts.STATUS_ACTIVE).Any())
                            {
                                continue;
                            }

                            IEnumerable<SubAdmissionCriterion> currentSubAdmissionCriterias = currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                                .Where(a => a.AdmissionMethodId == 1 && a.Status == Consts.STATUS_ACTIVE);
                            
                            IEnumerable<SubAdmissionCriterion> previousSubAdmissionCriterias = previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                                .Where(a => a.AdmissionMethodId == 1 && a.Status == Consts.STATUS_ACTIVE);

                            //Check ptts cho giới tính riêng
                            IEnumerable<SubAdmissionCriterion> subCurrentSubAdmissionCriteriasByGender = currentSubAdmissionCriterias.Where(s => s.Gender == subjectGroupParam.Gender);
                            if (subCurrentSubAdmissionCriteriasByGender.Any())
                            {
                                currentSubAdmissionCriterias = subCurrentSubAdmissionCriteriasByGender;
                            } else
                            {
                                currentSubAdmissionCriterias = currentSubAdmissionCriterias.Where(s => s.Gender == null);
                            }

                            IEnumerable<SubAdmissionCriterion> subPreviousSubAdmissionCriteriasByGender = previousSubAdmissionCriterias.Where(s => s.Gender == subjectGroupParam.Gender);
                            if (subPreviousSubAdmissionCriteriasByGender.Any())
                            {
                                previousSubAdmissionCriterias = subPreviousSubAdmissionCriteriasByGender;
                            } else
                            {
                                previousSubAdmissionCriterias = previousSubAdmissionCriterias.Where(s => s.Gender == null);

                            }

                            //Check ptts cho tỉnh riêng, chỉ có duy nhất 1 tiêu chí thỏa mãn
                            SubAdmissionCriterion subCurrentSubAdmissionCriteria = currentSubAdmissionCriterias.Where(s => s.ProvinceId == subjectGroupParam.ProvinceId).FirstOrDefault();
                            if (subCurrentSubAdmissionCriteria == null)
                            {
                                subCurrentSubAdmissionCriteria = currentSubAdmissionCriterias.Where(s => s.ProvinceId == null).FirstOrDefault(); ;
                            }

                            SubAdmissionCriterion subPreviousSubAdmissionCriteria = previousSubAdmissionCriterias.Where(s => s.ProvinceId == subjectGroupParam.ProvinceId).FirstOrDefault();
                            if (subPreviousSubAdmissionCriteria == null)
                            {
                                subPreviousSubAdmissionCriteria = previousSubAdmissionCriterias.Where(s => s.ProvinceId == null).FirstOrDefault();
                            }

                            if (subPreviousSubAdmissionCriteria == null || subCurrentSubAdmissionCriteria == null)
                            {
                                continue;
                            }

                            EntryMark currentEntryMark = (await _uow.EntryMarkRepository
                                    .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == subCurrentSubAdmissionCriteria.Id
                                        && e.MajorSubjectGroupId != null && e.MajorSubjectGroup.SubjectGroupId == subjectGroupDataSet.Id))
                                        .FirstOrDefault();

                            EntryMark previousEntryMark = (await _uow.EntryMarkRepository
                                    .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == subPreviousSubAdmissionCriteria.Id
                                    && e.MajorSubjectGroupId != null && e.MajorSubjectGroup.SubjectGroupId == subjectGroupDataSet.Id
                                    && e.Mark != null && e.Mark <= subjectGroupDataSet.TotalMark))
                                        .FirstOrDefault();


                            if (currentEntryMark == null || previousEntryMark == null)
                            {
                                continue;
                            }
                            isValid = true;
                            highestEntryMark = (double)previousEntryMark.Mark > highestEntryMark ? (double)previousEntryMark.Mark : highestEntryMark;
                        }

                        if (isValid)
                        {
                            MajorDataSet majorDataSet = _mapper.Map<MajorDataSet>(major);
                            majorDataSet.HighestEntryMark = highestEntryMark;
                            majorDataSets.Add(majorDataSet);
                        }
                    }

                    if (majorDataSets.Any())
                    {
                        subjectGroupDataSet.SuggestedMajors = majorDataSets;
                        suggestedSubjectGroups.Add(subjectGroupDataSet);
                    }
                }

                suggestedSubjectGroups = suggestedSubjectGroups
                    .OrderByDescending(o => o.TotalMark).Take(Consts.NUMBER_OF_SUGGESTED_GROUP).ToList();

                foreach (SubjectGroupDataSet suggestGroup in suggestedSubjectGroups)
                {
                    suggestGroup.SubjectDataSets = (await _uow.SubjecGroupDetailRepository
                            .Get(filter: s => s.SubjectId != null && s.SubjectGroupId == suggestGroup.Id,
                                includeProperties: "Subject")).Select(s => _mapper.Map<SubjectDataSet>(s.Subject)).ToList();
                    suggestGroup.SpecialSubjectGroupDataSets = (await _uow.SubjecGroupDetailRepository
                            .Get(filter: s => s.SpecialSubjectGroupId != null && s.SubjectGroupId == suggestGroup.Id,
                                 includeProperties: "SpecialSubjectGroup")).Select(s => _mapper.Map<SpecialSubjectGroupDataSet>(s.SpecialSubjectGroup)).ToList();
                    //Tính trọng số từng ngành
                    suggestGroup.SuggestedMajors = await GenerateListMajors(subjectGroupParam, suggestGroup.SuggestedMajors, suggestGroup.Id);
                }

                IEnumerable<SubjectGroupDataSet> results = suggestedSubjectGroups.Where(s => s.SuggestedMajors.Count() > 0);

                response.Succeeded = true;
                response.Data = results;
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

        private async Task<List<MajorDataSet>> GenerateListMajors(SubjectGroupParam subjectGroupParam,
            List<MajorDataSet> suggestedMajors, int subjectGroupId)
        {
            List<MajorDataSet> majorDataSets = new List<MajorDataSet>();
            List<MajorDataSet> majorDataSetsBaseOnEntryMark = new List<MajorDataSet>();
            foreach (MajorDataSet majorDataSet in suggestedMajors.OrderByDescending(e => e.HighestEntryMark))
            {
                Models.MajorSubjectGroup majorSubjectGroup = await _uow.MajorSubjectGroupRepository
                    .GetFirst(filter: m => m.SubjectGroupId == subjectGroupId && m.MajorId == majorDataSet.Id, includeProperties: "SubjectWeights");

                majorDataSet.WeightMark = await CalculateTotalWeightMark(subjectGroupParam, majorSubjectGroup.SubjectWeights);
                majorDataSets.Add(majorDataSet);
            }

            IEnumerable<IGrouping<double, MajorDataSet>> topMajorDataSetsGroups = majorDataSets.GroupBy(m => m.WeightMark)
                                                                                    .OrderByDescending(g => g.Key);
            foreach (IGrouping<double, MajorDataSet> topMajorDataSetsGroup in topMajorDataSetsGroups)
            {
                if (majorDataSetsBaseOnEntryMark.Count() < Consts.NUMBER_OF_SUGGESTED_MAJOR)
                {
                    majorDataSetsBaseOnEntryMark.AddRange(topMajorDataSetsGroup.AsEnumerable());
                }
                else
                {
                    break;
                }
            }

            return majorDataSetsBaseOnEntryMark;
        }

        public async Task<Response<IEnumerable<AdminSubjectGroupDataSet>>> GetListSubjectGroups()
        {
            Response<IEnumerable<AdminSubjectGroupDataSet>> response = new Response<IEnumerable<AdminSubjectGroupDataSet>>();
            try
            {
                IEnumerable<AdminSubjectGroupDataSet> subjectGroupDataSets = (await _uow.SubjectGroupRepository.Get(filter: s => s.Status == Consts.STATUS_ACTIVE))
                .Select(s => _mapper.Map<AdminSubjectGroupDataSet>(s));
                if (subjectGroupDataSets == null || !subjectGroupDataSets.Any())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không có khối nào trong hệ thống!");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = subjectGroupDataSets;
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

        private async Task<double> CalculateTotalWeightMark(SubjectGroupParam subjectGroupParam, IEnumerable<SubjectWeight> subjectWeights)
        {
            double totalMark = 0;
            double totalWeight = 0;
            foreach (SubjectWeight subjectWeight in subjectWeights)
            {
                double subjectMark = 0;
                SubjectGroupDetail subjectGroupDetail = await _uow.SubjecGroupDetailRepository.GetById(subjectWeight.SubjectGroupDetailId);
                if (subjectGroupDetail.SubjectId != null)
                {
                    MarkParam markParam = subjectGroupParam.Marks.FirstOrDefault(m => m.SubjectId == subjectGroupDetail.SubjectId);
                    if (markParam != null && markParam.Mark > 0)
                    {
                        subjectMark += markParam.Mark;
                    }
                    else
                    {
                        subjectMark += 0;
                    }
                }
                else if (subjectGroupDetail.SpecialSubjectGroupId != null)
                {
                    double totalSpecialGroupMark = 0;
                    IEnumerable<Models.Subject> subjects = (await _uow.SubjectRepository.Get(s => s.SpecialSubjectGroupId == subjectGroupDetail.SpecialSubjectGroupId));

                    if (subjects.Any())
                    {
                        foreach (Models.Subject subject in subjects)
                        {
                            MarkParam markParam = subjectGroupParam.Marks.FirstOrDefault(m => m.SubjectId == subject.Id);
                            if (markParam != null && markParam.Mark > 0)
                            {
                                totalSpecialGroupMark += markParam.Mark;
                            }
                            else
                            {
                                totalSpecialGroupMark += 0;
                            }
                        }
                        subjectMark += totalSpecialGroupMark / subjects.Count();
                    }
                }
                totalMark += subjectMark * subjectWeight.Weight;
                totalWeight += subjectWeight.Weight;
            }

            return Math.Round(totalMark / totalWeight, 2);
        }

        //Tính tổng điểm tổ hợp hôn
        private async Task<double> CalculateSubjectGroupMark(List<MarkParam> marks, List<SubjectGroupDetail> subjectGroupDetails)
        {
            double totalMark = 0;
            if (subjectGroupDetails == null || !subjectGroupDetails.Any())
            {
                return 0;
            }

            foreach (SubjectGroupDetail subjectGroupDetail in subjectGroupDetails)
            {
                if (subjectGroupDetail.SubjectId != null)
                {
                    MarkParam markParam = marks.FirstOrDefault(m => m.SubjectId == subjectGroupDetail.SubjectId);
                    if (markParam != null && markParam.Mark > 0)
                    {
                        totalMark += markParam.Mark;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (subjectGroupDetail.SpecialSubjectGroupId != null)
                {
                    double totalSpecialGroupMark = 0;
                    IEnumerable<Models.Subject> subjects = (await _uow.SubjectRepository.Get(s => s.SpecialSubjectGroupId == subjectGroupDetail.SpecialSubjectGroupId));

                    if (subjects.Any())
                    {
                        foreach (Models.Subject subject in subjects)
                        {
                            MarkParam markParam = marks.FirstOrDefault(m => m.SubjectId == subject.Id);
                            if (markParam != null && markParam.Mark > 0)
                            {
                                totalSpecialGroupMark += markParam.Mark;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        totalMark += (totalSpecialGroupMark / subjects.Count());
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return Math.Round(totalMark, 2);
        }

        public async Task<Response<CreateSubjectGroupDataset>> CreateNewSubjectGroup(CreateSubjectGroupParam createSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = new Response<CreateSubjectGroupDataset>();
            try
            {
                if (createSubjectGroupParam.GroupCode == null || createSubjectGroupParam.GroupCode.Trim().Equals(""))
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên khối không được để trống!");
                    return response;
                }
                Models.SubjectGroup existSubjectGroup = await _uow.SubjectGroupRepository.GetFirst(filter: e => e.GroupCode.Equals(createSubjectGroupParam.GroupCode));
                if (existSubjectGroup != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tên khối đã tồn tại trong hệ thống!");
                    return response;
                }


                List<SubjectDataSet> ListOfSubject = new List<SubjectDataSet>();

                if (createSubjectGroupParam.SubjectIds == null)
                {
                    createSubjectGroupParam.SubjectIds = new List<int?>();
                }
                if (createSubjectGroupParam.SpecicalSubjectGroupIds == null)
                {
                    createSubjectGroupParam.SpecicalSubjectGroupIds = new List<int?>();
                }

                if ((createSubjectGroupParam.SubjectIds.Count + createSubjectGroupParam.SpecicalSubjectGroupIds.Count) < Consts.REQUIRED_NUMBER_SUBJECTS)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Danh sách môn học không hợp lệ!");
                    return response;
                }
                if (createSubjectGroupParam.SubjectIds.Count() != createSubjectGroupParam.SubjectIds.Distinct().Count()
                    || createSubjectGroupParam.SpecicalSubjectGroupIds.Count() != createSubjectGroupParam.SpecicalSubjectGroupIds.Distinct().Count())
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Môn học trong một khối trùng nhau!");
                    return response;

                }
                foreach (int? id in createSubjectGroupParam.SpecicalSubjectGroupIds)
                {
                    if (id == null)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Môn học không thể là null");
                        return response;
                    }
                    Models.SpecialSubjectGroup specialSubjectGroup = await _uow.SpecialSubjectGroupRepository.GetById(id);
                    if (specialSubjectGroup == null)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Môn học không tồn tại trong hệ thống!");
                        return response;
                    }
                    SubjectDataSet subjectDataSet = new SubjectDataSet();
                    subjectDataSet.Id = specialSubjectGroup.Id;
                    subjectDataSet.Name = specialSubjectGroup.Name;
                    subjectDataSet.SpecialSubjectGroupId = specialSubjectGroup.Id;
                    ListOfSubject.Add(subjectDataSet);
                }

                foreach (int? id in createSubjectGroupParam.SubjectIds)
                {
                    if (id == null)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Môn học không thể là null");
                        return response;
                    }
                    Models.Subject subject = await _uow.SubjectRepository.GetById(id);
                    if (subject == null)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Môn học không tồn tại trong hệ thống!");
                        return response;
                    }
                    SubjectDataSet subjectDataSet = new SubjectDataSet();
                    subjectDataSet.Id = subject.Id;
                    subjectDataSet.Name = subject.Name;
                    ListOfSubject.Add(subjectDataSet);
                }




                IEnumerable<IGrouping<int, Models.SubjectGroupDetail>> foundSubjectGroups = (await _uow.SubjecGroupDetailRepository.
                     Get(filter: s => createSubjectGroupParam.SpecicalSubjectGroupIds.Contains(s.SpecialSubjectGroupId) || createSubjectGroupParam.SubjectIds.Contains(s.SubjectId))).
                     GroupBy(g => g.SubjectGroupId).Where(g => g.Count() == (createSubjectGroupParam.SpecicalSubjectGroupIds.Count() + createSubjectGroupParam.SubjectIds.Count()));

                IEnumerable<int> foundSubjectGroupIds = foundSubjectGroups.Select(s => s.Key);

                foreach (int id in foundSubjectGroupIds)
                {
                    bool isExisted = (await _uow.SubjecGroupDetailRepository.Get(
                        filter: s => s.SubjectGroupId == id)).Count() ==
                        (createSubjectGroupParam.SpecicalSubjectGroupIds.Count() + createSubjectGroupParam.SubjectIds.Count());

                    if (isExisted)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Khối có những môn học trên đã tồn tại trong hệ thống!");
                        return response;
                    }
                }
                Models.SubjectGroup insertSubjectGroupModels = new Models.SubjectGroup
                {
                    GroupCode = createSubjectGroupParam.GroupCode,
                    Status = Consts.STATUS_ACTIVE
                };
                _uow.SubjectGroupRepository.Insert(insertSubjectGroupModels);
                int result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Thêm khối bị lỗi!");
                    return response;
                }

                foreach (int specicalSubjectGroupId in createSubjectGroupParam.SpecicalSubjectGroupIds)
                {

                    Models.SubjectGroupDetail insertSubjectGroupDetailModel = new SubjectGroupDetail();
                    insertSubjectGroupDetailModel.SubjectGroupId = insertSubjectGroupModels.Id;
                    insertSubjectGroupDetailModel.SpecialSubjectGroupId = specicalSubjectGroupId;

                    _uow.SubjecGroupDetailRepository.Insert(insertSubjectGroupDetailModel);
                    result = await _uow.CommitAsync();
                    if (result <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Thêm chi tiết của khối bị lỗi!");
                        return response;
                    }
                }

                foreach (int subjectId in createSubjectGroupParam.SubjectIds)
                {

                    Models.SubjectGroupDetail insertSubjectGroupDetailModel = new SubjectGroupDetail();
                    insertSubjectGroupDetailModel.SubjectGroupId = insertSubjectGroupModels.Id;
                    insertSubjectGroupDetailModel.SubjectId = subjectId;

                    _uow.SubjecGroupDetailRepository.Insert(insertSubjectGroupDetailModel);
                    result = await _uow.CommitAsync();
                    if (result <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Thêm chi tiết của khối bị lỗi!");
                        return response;
                    }
                }
                CreateSubjectGroupDataset createSubjectGroupDataset = new CreateSubjectGroupDataset
                {
                    Id = insertSubjectGroupModels.Id,
                    GroupCode = insertSubjectGroupModels.GroupCode,
                    Status = Consts.STATUS_ACTIVE,
                    ListOfSubject = ListOfSubject,
                };
                response.Succeeded = true;
                response.Data = createSubjectGroupDataset;
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

        public async Task<Response<CreateSubjectGroupDataset>> UpdateSubjectGroup(UpdateSubjectGroupParam updateSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = new Response<CreateSubjectGroupDataset>();
            //int id = updateSubjectGroupParam.Id;
            //List<SubjectGroupDetailParam> listOfSubject = updateSubjectGroupParam.ListOfSubject;
            //if (updateSubjectGroupParam.Id < 1)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Id không hợp lệ!");
            //    return response;
            //}
            //if (listOfSubject == null || listOfSubject.Count < Consts.REQUIRED_NUMBER_SUBJECTS)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Số môn học phải lớn hơn hoặc bằng "+ Consts.REQUIRED_NUMBER_SUBJECTS+ "!");
            //    return response;
            //}
            //if (updateSubjectGroupParam.GroupCode == null || updateSubjectGroupParam.GroupCode.Trim().Equals(""))
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tên khối không được để trống!");
            //    return response;
            //}
            //Models.SubjectGroup existSubjectGroupByCode = await _uow.SubjectGroupRepository.GetFirst(filter: e => e.GroupCode.Equals(updateSubjectGroupParam.GroupCode));
            //if (existSubjectGroupByCode != null && existSubjectGroupByCode.Id != updateSubjectGroupParam.Id)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tên khối đã tồn tại trong hệ thống!");
            //    return response;
            //}//XONG



            //List<SubjectDataSet> ListOfSubject = new List<SubjectDataSet>();
            //List<int?> normalSubjects = new List<int?>();
            //List<int?> specicalSubjects = new List<int?>();
            //foreach (SubjectGroupDetailParam subject in listOfSubject)
            //{
            //    SubjectDataSet subjectDataSet = new SubjectDataSet();
            //    if (subject.IsSpecicSubject)
            //    {
            //        specicalSubjects.Add(subject.SubjectId);


            //        Models.SpecialSubjectGroup specialSubjectGroup = await _uow.SpecialSubjectGroupRepository.GetById(subject.SubjectId);
            //        if (specialSubjectGroup == null)
            //        {
            //            response.Succeeded = false;
            //            if (response.Errors == null)
            //            {
            //                response.Errors = new List<string>();
            //            }
            //            response.Errors.Add("Môn học không tồn tại trong hệ thống!");
            //            return response;
            //        }
            //        subjectDataSet.Id = specialSubjectGroup.Id;
            //        subjectDataSet.Name = specialSubjectGroup.Name;
            //        subjectDataSet.SpecialSubjectGroupId = specialSubjectGroup.Id;
            //    }
            //    else
            //    {
            //        normalSubjects.Add(subject.SubjectId);

            //        Models.Subject sj = await _uow.SubjectRepository.GetById(subject.SubjectId);
            //        if (sj == null)
            //        {
            //            response.Succeeded = false;
            //            if (response.Errors == null)
            //            {
            //                response.Errors = new List<string>();
            //            }
            //            response.Errors.Add("Môn học không tồn tại trong hệ thống!");
            //            return response;
            //        }
            //        subjectDataSet.Id = sj.Id;
            //        subjectDataSet.Name = sj.Name;
            //    }
            //    ListOfSubject.Add(subjectDataSet);
            //}




            //IEnumerable<IGrouping<int, Models.SubjectGroupDetail>> foundedSubjectGroups = (await _uow.SubjecGroupDetailRepository.
            //     Get(filter: s => specicalSubjects.Contains(s.SpecialSubjectGroupId) || normalSubjects.Contains(s.SubjectId))).
            //     GroupBy(g => g.SubjectGroupId).Where(g => g.Count() == listOfSubject.Count());
            //IEnumerable<int> foundedSubjectGroupIds = foundedSubjectGroups.Select(s => s.Key);



            //foreach (int subjectGroupId in foundedSubjectGroupIds)
            //{
            //    bool isExisted = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == subjectGroupId)).Count() == listOfSubject.Count;
            //    if (isExisted && subjectGroupId != updateSubjectGroupParam.Id)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Khối bạn muốn cập nhập đã tồn tại trong hệ thống!");
            //        return response;
            //    }
            //}

            //Models.SubjectGroup updateSubjectGroupModel = await _uow.SubjectGroupRepository.GetById(id);

            //updateSubjectGroupModel.GroupCode = updateSubjectGroupParam.GroupCode;
            //updateSubjectGroupModel.Status = updateSubjectGroupParam.Status;

            //_uow.SubjectGroupRepository.Update(updateSubjectGroupModel);
            //int result = await _uow.CommitAsync();
            //if (result <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Cập nhật khối bị lỗi!");
            //    return response;
            //}

            //_uow.SubjecGroupDetailRepository.DeleteComposite(filter: s => s.SubjectGroupId == updateSubjectGroupModel.Id);
            //await _uow.CommitAsync();

            //foreach (SubjectGroupDetailParam subjectParam in listOfSubject)
            //{

            //    Models.SubjectGroupDetail insertSubjectGroupDetailModel = new SubjectGroupDetail();

            //    if (subjectParam.IsSpecicSubject)
            //    {
            //        insertSubjectGroupDetailModel.SubjectGroupId = updateSubjectGroupModel.Id;
            //        insertSubjectGroupDetailModel.SpecialSubjectGroupId = subjectParam.SubjectId;
            //    }
            //    else
            //    {
            //        insertSubjectGroupDetailModel.SubjectGroupId = insertSubjectGroupModels.Id;
            //        insertSubjectGroupDetailModel.SubjectId = subjectParam.SubjectId;
            //    }

            //    _uow.SubjecGroupDetailRepository.Insert(insertSubjectGroupDetailModel);
            //    result = await _uow.CommitAsync();
            //    if (result <= 0)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Thêm chi tiết của khối bị lỗi!");
            //        return response;
            //    }
            //}
            //List<Models.Subject> subjects = (await _uow.SubjectRepository.Get(filter: s => listOfSubjectId.Contains(s.Id))).ToList();
            //List<SubjectDataSet> subjectDatas = new List<SubjectDataSet>();
            //foreach (Models.Subject subject in subjects)
            //{
            //    subjectDatas.Add(_mapper.Map<SubjectDataSet>(subject));
            //}
            //CreateSubjectGroupDataset updateSubjectGroupDataset = new CreateSubjectGroupDataset
            //{
            //    Id = updateSubjectGroupParam.Id,
            //    GroupCode = updateSubjectGroupParam.GroupCode,
            //    ListOfSubject = subjectDatas,
            //    Status = updateSubjectGroupParam.Status
            //};

            //response.Succeeded = true;
            //response.Data = updateSubjectGroupDataset;
            return response;
        }

        public async Task<Response<UserSuggestionInformation>> GetUserSuggestTopSubjectGroup(string token)
        {
            Response<UserSuggestionInformation> response = new Response<UserSuggestionInformation>();
            try
            {
                UserSuggestionInformation userSuggestionSubjectGroup = null;
                if (token == null || token.Trim().Length == 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

                if (userIdString == null || userIdString.Length <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn không tồn tại!");
                    return response;
                }

                int userId = Int32.Parse(userIdString);

                Models.User user = await _uow.UserRepository.GetFirst(filter: u => u.Id == userId && u.IsActive == true,
                                                                    includeProperties: "Transcripts.TranscriptType");
                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn không tồn tại!");
                    return response;
                }

                if (!user.Transcripts.Any() || user.ProvinceId == null || user.Gender == null)
                {
                    response.Succeeded = true;
                    return response;
                }

                var transcriptGroups = user.Transcripts.GroupBy(s => s.TranscriptType).OrderByDescending(t => t.Key.Priority);
                foreach (var group in transcriptGroups)
                {
                    List<MarkParam> marks = new List<MarkParam>();
                    foreach (Models.Transcript transcript in group)
                    {
                        marks.Add(new MarkParam()
                        {
                            Mark = transcript.Mark,
                            SubjectId = transcript.SubjectId
                        });
                    }
                    SubjectGroupParam param = new SubjectGroupParam()
                    {
                        Gender = (int)user.Gender,
                        ProvinceId = (int)user.ProvinceId,
                        TranscriptTypeId = group.Key.Id,
                        Marks = marks
                    };
                    Response<IEnumerable<SubjectGroupDataSet>> subjectGroupReponse = await GetCaculatedSubjectGroup(param);
                    if (!subjectGroupReponse.Succeeded)
                    {
                        continue;
                    }
                    if (!subjectGroupReponse.Data.Any())
                    {
                        continue;
                    }
                    bool isValid = false;
                    foreach (var subjectGroup in subjectGroupReponse.Data)
                    {
                        if (subjectGroup.SuggestedMajors.Any())
                        {
                            isValid = true;
                            break;
                        }
                    }
                    if (!isValid)
                    {
                        continue;
                    }
                    userSuggestionSubjectGroup = new UserSuggestionInformation()
                    {
                        TranscriptTypeId = group.Key.Id,
                        TranscriptTypeName = group.Key.Name,
                        SubjectGroupDataSets = subjectGroupReponse.Data,
                        TranscriptDetails = await _uow.TranscriptRepository.GetUserTranscripts(userId),
                        Gender = user.Gender,
                        ProvinceId = user.ProvinceId
                    };
                    break;
                }
                response.Succeeded = true;
                response.Data = userSuggestionSubjectGroup;
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

        public async Task<Response<SubjectGroupResponseDataSet>> GetSubjectGroupWeight(int id)
        {
            Response<SubjectGroupResponseDataSet> response = new Response<SubjectGroupResponseDataSet>();
            try
            {
                Models.SubjectGroup subjectGroup = await _uow.SubjectGroupRepository.GetById(id);
                if (subjectGroup == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không thể tìm thấy khối!");
                    return response;
                }

                List<Models.SubjectGroupDetail> subjectGroupDetails =
                    (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == id,
                    includeProperties: "Subject,SpecialSubjectGroup"))?.ToList();

                if (subjectGroupDetails == null || subjectGroupDetails.Count() == 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Không có môn học tương ứng với khối!");
                    return response;
                }
                List<SubjectResponseDataSet> subjectResponseDataSets = new List<SubjectResponseDataSet>();
                SubjectGroupWeightDataSet subjectGroupWeightDataSet = new SubjectGroupWeightDataSet();

                foreach (var item in subjectGroupDetails)
                {
                    bool isSpecialGroup = false;
                    string name = "";
                    int newId = 0;
                    if (item.SpecialSubjectGroupId != null)
                    {
                        isSpecialGroup = true;
                        name = item.SpecialSubjectGroup.Name;
                        newId = item.SpecialSubjectGroup.Id;
                    }
                    else
                    {
                        name = item.Subject.Name;
                        newId = item.Subject.Id;
                    }
                    SubjectResponseDataSet subjectResponse = new SubjectResponseDataSet()
                    {
                        Id = newId,
                        Name = name,
                        IsSpecialSubjectGroup = isSpecialGroup
                    };
                    subjectResponseDataSets.Add(subjectResponse);
                }
                SubjectGroupResponseDataSet subjectGroupResponseDataSet = new SubjectGroupResponseDataSet()
                {
                    Id = id,
                    GroupCode = subjectGroup.GroupCode,
                    Subjects = subjectResponseDataSets
                };

                response.Succeeded = true;
                response.Data = subjectGroupResponseDataSet;

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
