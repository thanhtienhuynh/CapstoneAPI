using AutoMapper;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.SpecialSubjectGroup;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.SubjectGroup
{
    public class SubjectGroupService : ISubjectGroupService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public SubjectGroupService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<SubjectGroupDataSet>>> GetCaculatedSubjectGroup(SubjectGroupParam subjectGroupParam)
        {
            Response<IEnumerable<SubjectGroupDataSet>> response = new Response<IEnumerable<SubjectGroupDataSet>>();
            List<SubjectGroupDataSet> subjectGroupDataSets = new List<SubjectGroupDataSet>();
            //Lấy danh sách khối
            IEnumerable<Models.SubjectGroup> subjectGroups = await _uow.SubjectGroupRepository.Get(includeProperties: "SubjectGroupDetails,SubjectGroupDetails.Subject,SubjectGroupDetails.SpecialSubjectGroup");

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
                    subjectGroupDataSets.Add(new SubjectGroupDataSet {
                        TotalMark = totalMark,
                        Name = subjectGroup.GroupCode,
                        Id = subjectGroup.Id,
                        SubjectDataSets = subjectGroup.SubjectGroupDetails.Where(s => s.Subject != null).Select(s => _mapper.Map<SubjectDataSet>(s.Subject)).ToList(),
                        SpecialSubjectGroupDataSets = subjectGroup.SubjectGroupDetails.Where(s => s.SpecialSubjectGroup != null).Select(s => _mapper.Map<SpecialSubjectGroupDataSet>(s.SpecialSubjectGroup)).ToList()
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
                    .Get(filter: s => s.SubjectGroupId == subjectGroupDataSet.Id,
                        includeProperties: "Major")).Select(s => s.Major);
                List<MajorDataSet> majorDataSets = new List<MajorDataSet>();
                foreach (Models.Major major in majors)
                {

                    var groupsByMajorDetails = (await _uow.MajorDetailRepository
                        .Get(filter: m => m.MajorId == major.Id && m.Status == Consts.STATUS_ACTIVE,
                            includeProperties: "University,TrainingProgram,AdmissionCriterion,AdmissionCriterion.SubAdmissionCriteria"))
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
                            || !currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.Status == Consts.STATUS_ACTIVE).Any()
                            || previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria == null
                            || !previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.Status == Consts.STATUS_ACTIVE).Any())
                        {
                            continue;
                        }

                        List<SubAdmissionCriterion> currentSubAdmissionCriterias = currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                            .Where(a => a.Status == Consts.STATUS_ACTIVE && a.AdmissionMethodId == 1 && (a.Gender == subjectGroupParam.Gender || a.Gender == null)
                             && (a.ProvinceId == subjectGroupParam.ProvinceId || a.ProvinceId == null)).ToList();
                        List<SubAdmissionCriterion> previousSubAdmissionCriterias = previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                            .Where(a => a.Status == Consts.STATUS_ACTIVE && a.AdmissionMethodId == 1 && (a.Gender == subjectGroupParam.Gender || a.Gender == null)
                             && (a.ProvinceId == subjectGroupParam.ProvinceId || a.ProvinceId == null)).ToList();

                        if (!currentSubAdmissionCriterias.Any() || !previousSubAdmissionCriterias.Any())
                        {
                            continue;
                        }

                        List<EntryMark> currentEntryMarks = new List<EntryMark>();
                        List<EntryMark> previousEntryMarks = new List<EntryMark>();

                        foreach (SubAdmissionCriterion currentSubAdmissionCriteria in currentSubAdmissionCriterias)
                        {
                            List<EntryMark> entryMarks = (await _uow.EntryMarkRepository
                                .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == currentSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
                                    includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup,SubAdmissionCriterion"))
                                .ToList();
                            currentEntryMarks.AddRange(entryMarks);
                        }

                        foreach (SubAdmissionCriterion previousSubAdmissionCriteria in previousSubAdmissionCriterias)
                        {
                            List<EntryMark> entryMarks = (await _uow.EntryMarkRepository
                                .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == previousSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
                                    includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup,SubAdmissionCriterion"))
                                .ToList();
                            previousEntryMarks.AddRange(entryMarks);
                        }

                        if (!currentEntryMarks.Any() || !previousEntryMarks.Any())
                        {
                            continue;
                        }

                        if (!currentEntryMarks.Where(e => e.MajorSubjectGroup.SubjectGroupId == subjectGroupDataSet.Id).Any()
                            || !previousEntryMarks.Where(e => e.MajorSubjectGroup.SubjectGroupId == subjectGroupDataSet.Id
                                                        && e.Mark != null
                                                        && e.Mark <= subjectGroupDataSet.TotalMark).Any())
                        {
                            continue;
                        }
                        isValid = true;
                        double newMark = previousEntryMarks.First(e => e.MajorSubjectGroup.SubjectGroupId == subjectGroupDataSet.Id).Mark ?? 0;
                        highestEntryMark = newMark > highestEntryMark ? newMark : highestEntryMark;
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
                //Tính trọng số từng ngành
                suggestGroup.SuggestedMajors = await GenerateListMajors(subjectGroupParam, suggestGroup.SuggestedMajors, suggestGroup.Id);
            }

            IEnumerable<SubjectGroupDataSet> results = suggestedSubjectGroups.Where(s => s.SuggestedMajors.Count() > 0);

            response.Succeeded = true;
            response.Data = suggestedSubjectGroups.Where(s => s.SuggestedMajors.Count() > 0);
            return response;
        }

        private async Task<List<MajorDataSet>> GenerateListMajors(SubjectGroupParam subjectGroupParam,
            List<MajorDataSet> suggestedMajors, int subjectGroupId)
        {
            List<MajorDataSet> majorDataSets = new List<MajorDataSet>();
            List<MajorDataSet> majorDataSetsBaseOnEntryMark = new List<MajorDataSet>();
            foreach (MajorDataSet majorDataSet in suggestedMajors)
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
                        subjectMark += (totalSpecialGroupMark / subjects.Count());
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
                    } else
                    {
                        return 0;
                    }
                } else
                {
                    return 0;
                }
            }
            return Math.Round(totalMark, 2);
        }

        public async Task<Response<CreateSubjectGroupDataset>> CreateNewSubjectGroup(CreateSubjectGroupParam createSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = new Response<CreateSubjectGroupDataset>();
            //List<int> listOfSubjectId = createSubjectGroupParam.ListOfSubjectId;
            //if (listOfSubjectId == null || listOfSubjectId.Count < 3)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Danh sách môn học không hợp lệ!");
            //    return response;
            //}

            //if (createSubjectGroupParam.GroupCode == null || createSubjectGroupParam.GroupCode.Trim().Equals(""))
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tên khối không được để trống!");
            //    return response;
            //}
            //Models.SubjectGroup existSubjectGroup = await _uow.SubjectGroupRepository.GetFirst(filter: e => e.GroupCode.Equals(createSubjectGroupParam.GroupCode));
            //if (existSubjectGroup != null)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tên khối đã tồn tại trong hệ thống!");
            //    return response;
            //}

            //IEnumerable<int> foundedSubjectGroupIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => listOfSubjectId.Contains(s.SubjectId)))
            //    .GroupBy(s => s.SubjectGroupId).Where(g => g.Count() == listOfSubjectId.Count()).Select(g => g.Key);

            //foreach (int id in foundedSubjectGroupIds)
            //{
            //    bool isExisted = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == id)).Count() == listOfSubjectId.Count;
            //    if (isExisted)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Khối có những môn học trên đã tồn tại trong hệ thống!");
            //        return response;
            //    }
            //}
            //Models.SubjectGroup insertSubjectGroupModels = new Models.SubjectGroup
            //{
            //    GroupCode = createSubjectGroupParam.GroupCode,
            //    Status = Consts.STATUS_ACTIVE
            //};
            //_uow.SubjectGroupRepository.Insert(insertSubjectGroupModels);
            //int result = await _uow.CommitAsync();
            //if (result <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Lỗi hệ thống!");
            //    return response;
            //}
            //foreach (int subjectId in listOfSubjectId)
            //{
            //    Models.SubjectGroupDetail insertSubjectGroupDetailModel = new SubjectGroupDetail
            //    {
            //        SubjectGroupId = insertSubjectGroupModels.Id,
            //        SubjectId = subjectId,
            //    };
            //    _uow.SubjecGroupDetailRepository.Insert(insertSubjectGroupDetailModel);
            //    result = await _uow.CommitAsync();
            //    if (result <= 0)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Lỗi hệ thống!");
            //        return response;
            //    }
            //}
            //List<Models.Subject> subjects = (await _uow.SubjectRepository.Get(filter: s => listOfSubjectId.Contains(s.Id))).ToList();
            //List<SubjectDataSet> subjectDatas = new List<SubjectDataSet>();
            //foreach (Models.Subject subject in subjects)
            //{
            //    subjectDatas.Add(_mapper.Map<SubjectDataSet>(subject));
            //}
            //CreateSubjectGroupDataset createSubjectGroupDataset = new CreateSubjectGroupDataset
            //{
            //    Id = insertSubjectGroupModels.Id,
            //    GroupCode = createSubjectGroupParam.GroupCode,
            //    ListOfSubject = subjectDatas,
            //    Status = insertSubjectGroupModels.Status
            //};
            //response.Succeeded = true;
            //response.Data = createSubjectGroupDataset;
            return response;

        }

        public async Task<Response<CreateSubjectGroupDataset>> UpdateSubjectGroup(UpdateSubjectGroupParam updateSubjectGroupParam)
        {
            Response<CreateSubjectGroupDataset> response = new Response<CreateSubjectGroupDataset>();
            //int id = updateSubjectGroupParam.Id;
            //List<int> listOfSubjectId = updateSubjectGroupParam.ListOfSubjectId;
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
            //if (updateSubjectGroupParam.ListOfSubjectId.Count != updateSubjectGroupParam.ListOfSubjectId.Distinct().Count())
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Không được có môn trùng nhau!");
            //    return response;
            //}
            //if (listOfSubjectId == null || listOfSubjectId.Count < Consts.REQUIRED_NUMBER_SUBJECTS)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Số môn học phải lớn hơn hoặc bằng 3!");
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
            //}

            //IEnumerable<int> foundedSubjectGroupIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => listOfSubjectId.Contains(s.SubjectId)))
            //    .GroupBy(s => s.SubjectGroupId).Where(g => g.Count() == listOfSubjectId.Count()).Select(g => g.Key);
            //foreach (int subjectGroupId in foundedSubjectGroupIds)
            //{
            //    bool isExisted = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == subjectGroupId)).Count() == listOfSubjectId.Count;
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
            //    response.Errors.Add("Lỗi hệ thống!");
            //    return response;
            //}

            //IEnumerable<int> oldSubjectIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == updateSubjectGroupModel.Id))
            //    .Select(s => s.SubjectId);
            //foreach (int oldSubjectId in oldSubjectIds)
            //{
            //    _uow.SubjecGroupDetailRepository.DeleteComposite(filter: s => s.SubjectId == oldSubjectId && s.SubjectGroupId == updateSubjectGroupModel.Id);
            //}

            //await _uow.CommitAsync();

            //foreach (int subjectId in listOfSubjectId)
            //{
            //    Models.SubjectGroupDetail insertSubjectGroupDetailModel = new SubjectGroupDetail
            //    {
            //        SubjectGroupId = id,
            //        SubjectId = subjectId,
            //    };
            //    _uow.SubjecGroupDetailRepository.Insert(insertSubjectGroupDetailModel);
            //    result = await _uow.CommitAsync();
            //    if (result <= 0)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Lỗi hệ thống!");
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
    }
}
