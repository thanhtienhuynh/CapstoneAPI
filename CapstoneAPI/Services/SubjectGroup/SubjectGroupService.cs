﻿using AutoMapper;
using CapstoneAPI.DataSets.Major;
using CapstoneAPI.DataSets.Subject;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
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

        public async Task<IEnumerable<SubjectGroupDataSet>> GetCaculatedSubjectGroup(SubjectGroupParam subjectGroupParam)
        {
            List<SubjectGroupDataSet> subjectGroupDataSets = new List<SubjectGroupDataSet>();
            //Lấy danh sách khối
            IEnumerable<Models.SubjectGroup> subjectGroups = await _uow.SubjectGroupRepository.Get(includeProperties: "SubjectGroupDetails");
            
            //Tính điểm mỗi khối
            foreach (Models.SubjectGroup subjectGroup in subjectGroups)
            {
                double totalMark = CalculateSubjectGroupMark(subjectGroupParam, subjectGroup.SubjectGroupDetails.ToList());
                if (totalMark > 0)
                {
                    subjectGroupDataSets.Add(new SubjectGroupDataSet { TotalMark = totalMark, Name = subjectGroup.GroupCode, Id = subjectGroup.Id });
                }
            }

            if (!subjectGroupParam.IsSuggest)
            {
                return subjectGroupDataSets.OrderByDescending(o => o.TotalMark).ToList();
            }

            if (!subjectGroupDataSets.Any())
            {
                return null;
            }
            
            //Lọc những khối không có ngành phù hợp
            foreach(SubjectGroupDataSet subjectGroupDataSet in subjectGroupDataSets.ToList())
            {
                bool isValid = (await _uow.EntryMarkRepository.Get(filter: e => e.SubjectGroupId == subjectGroupDataSet.Id
                                                            && e.Year == Consts.NEAREST_YEAR
                                                            && e.Mark >= subjectGroupDataSet.TotalMark)).Any();
                if(!isValid)
                {
                    subjectGroupDataSets.Remove(subjectGroupDataSet);
                }
            }

            IEnumerable<SubjectGroupDataSet> suggestedSubjectGroups = subjectGroupDataSets
                .OrderByDescending(o => o.TotalMark).Take(Consts.NUMBER_OF_SUGGESTED_GROUP).ToList();

            foreach (SubjectGroupDataSet suggestGroup in suggestedSubjectGroups)
            {
                List<int> majorIds = (await _uow.WeightNumberRepository.
                                                            Get(filter: weightNumbers => weightNumbers.SubjectGroupId == suggestGroup.Id, includeProperties: "Major"))
                                                            .Where(w => w.Major.Status == Consts.STATUS_ACTIVE)
                                                            .Select(w => w.MajorId).Distinct().ToList();
 

                //Lọc những id ngành không có trường phù hợp vì thấp hơn điểm chuẩn
                await FilterMajorsWithEntryMark(majorIds, suggestGroup);

                //Tính trọng số từng ngành
                suggestGroup.SuggestedMajors = await GenerateListMajors(subjectGroupParam, suggestGroup, majorIds);
            }

            return suggestedSubjectGroups.Where(s => s.SuggestedMajors.Count() > 0);
        }

        //Lọc ra các ngành không phù hợp vì tổng điểm của khối thấp hơn điểm chuẩn
        private async Task FilterMajorsWithEntryMark(List<int> majorIds, SubjectGroupDataSet subjectGroup)
        {
            List<MajorDataSet> filteredMajors = new List<MajorDataSet>();
            foreach (int majorId in majorIds.ToList())
            {
                List<int> majorDetailIds = (await _uow.MajorDetailRepository.Get(filter: m => m.MajorId == majorId)).Select(m => m.Id).ToList();
                bool isSuitable = (await _uow.EntryMarkRepository.Get(filter: e => majorDetailIds.Contains(e.MajorDetailId) && e.Mark > 0 && e.Mark <= subjectGroup.TotalMark)).Any();
                if (!isSuitable)
                {
                    majorIds.Remove(majorId);
                }
            }
        }

        private async Task<List<MajorDataSet>> GenerateListMajors(SubjectGroupParam subjectGroupParam, 
            SubjectGroupDataSet suggestGroup, List<int> majorIds)
        {
            List<MajorDataSet> majorDataSets = new List<MajorDataSet>();
            List<MajorDataSet> majorDataSetsBaseOnEntryMark = new List<MajorDataSet>();
            foreach (int majorId in majorIds)
            {
                IEnumerable<WeightNumber> weightNumbers = await _uow.WeightNumberRepository
                    .Get(w => w.MajorId == majorId && w.SubjectGroupId == suggestGroup.Id);
                MajorDataSet major = _mapper.Map<MajorDataSet>(await _uow.MajorRepository.GetById(majorId));
                major.WeightMark = CalculateTotalWeightMark(subjectGroupParam, weightNumbers);
                majorDataSets.Add(major);
            }

            IEnumerable<IGrouping<double, MajorDataSet>> topMajorDataSetsGroups = majorDataSets.GroupBy(m => m.WeightMark)
                                                                                    .OrderByDescending(g => g.Key);
            foreach (IGrouping<double, MajorDataSet> topMajorDataSetsGroup in topMajorDataSetsGroups)
            {
                if (majorDataSetsBaseOnEntryMark.Count() < Consts.NUMBER_OF_SUGGESTED_MAJOR)
                {
                    majorDataSetsBaseOnEntryMark.AddRange(topMajorDataSetsGroup.AsEnumerable());
                } else
                {
                    break;
                }
            }

            foreach(MajorDataSet majorDataSet in majorDataSetsBaseOnEntryMark)
            {
                //Lấy điểm chuẩn cao nhất của năm gần nhất của ngành đó của các trường
                List<EntryMark> entryMarks = (await _uow.MajorDetailRepository.Get(filter: m => m.MajorId == majorDataSet.Id, includeProperties: "EntryMarks"))
                                                     .Select(m => m.EntryMarks.OrderByDescending(e => e.Mark).Where(e => e.Year == Consts.NEAREST_YEAR && e.SubjectGroupId == suggestGroup.Id).FirstOrDefault())
                                                     .Where(e => e != null).ToList();
                majorDataSet.HighestEntryMark = entryMarks.OrderByDescending(e => e.Mark ?? default(double)).First().Mark ?? default(double);
            }
            majorDataSetsBaseOnEntryMark = majorDataSetsBaseOnEntryMark.OrderByDescending(m => m.HighestEntryMark).ToList();
            
            if (majorDataSetsBaseOnEntryMark.Count() > Consts.NUMBER_OF_SUGGESTED_MAJOR)
            {
                double baseEntryMark = majorDataSetsBaseOnEntryMark[Consts.NUMBER_OF_SUGGESTED_MAJOR - 1].HighestEntryMark;
                majorDataSetsBaseOnEntryMark = majorDataSetsBaseOnEntryMark.Where(m => m.HighestEntryMark >= baseEntryMark).ToList();
            }
            return majorDataSetsBaseOnEntryMark;
        }

        public async Task<IEnumerable<AdminSubjectGroupDataSet>> GetListSubjectGroups()
        {
            return (await _uow.SubjectGroupRepository.Get(filter: s => s.Status == Consts.STATUS_ACTIVE))
                .Select(s => _mapper.Map<AdminSubjectGroupDataSet>(s));
        }

        private double CalculateTotalWeightMark(SubjectGroupParam subjectGroupParam, IEnumerable<WeightNumber> weightNumbers)
        {
            double totalMark = 0;
            double totalWeight = 0;
            foreach (WeightNumber weightNumber in weightNumbers)
            {
                totalMark += CalculateWeightMark(subjectGroupParam, weightNumber);
                totalWeight += weightNumber.Weight == null ? 1.0 : (double) weightNumber.Weight;
            }

            return Math.Round(totalMark / totalWeight, 2);
        }

        private double CalculateWeightMark(SubjectGroupParam subjectGroupParam, WeightNumber weightNumber)
        {
            if (weightNumber.SubjectId == null)
            {
                return 0;
            }

            if (weightNumber.Weight == null || weightNumber.Weight <= 0)
            {
                weightNumber.Weight = 1;
            }

            foreach (MarkParam markParam in subjectGroupParam.Marks)
            {
                if (markParam.SubjectId == weightNumber.SubjectId)
                {
                    return (double)(markParam.Mark * weightNumber.Weight);
                }
            }
            return 0;
        }

        //Tính tổng điểm tổ hợp hôn
        private double CalculateSubjectGroupMark(SubjectGroupParam subjectGroupParam, List<SubjectGroupDetail> subjectGroupDetails)
        {
            double totalMark = 0;
            if (subjectGroupDetails.Count != 3)
            {
                return 0;
            }
            foreach(SubjectGroupDetail subjectGroupDetail in subjectGroupDetails)
            {
                MarkParam markParam =  subjectGroupParam.Marks.FirstOrDefault(m => m.SubjectId == subjectGroupDetail.SubjectId);
                if (markParam != null && markParam.Mark > 0 )
                {
                    totalMark += markParam.Mark;
                } else
                {
                    return 0;
                }
            } 
            return totalMark;
        }

        public async Task<CreateSubjectGroupDataset> CreateNewSubjectGroup(CreateSubjectGroupParam createSubjectGroupParam)
        {
            List<int> listOfSubjectId = createSubjectGroupParam.ListOfSubjectId;
            if(listOfSubjectId == null || listOfSubjectId.Count <3)
            {
                return null;
            }
            if(createSubjectGroupParam.GroupCode == null || createSubjectGroupParam.GroupCode.Equals(""))
            {
                return null;
            }
            Models.SubjectGroup existSubjectGroup = await _uow.SubjectGroupRepository.GetFirst(filter: e => e.GroupCode.Equals(createSubjectGroupParam.GroupCode));
            if(existSubjectGroup != null)
            {
                return null;
            }

            IEnumerable<int> foundedSubjectGroupIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => listOfSubjectId.Contains(s.SubjectId)))
                .GroupBy(s => s.SubjectGroupId).Where(g => g.Count() == listOfSubjectId.Count()).Select(g => g.Key);

            foreach (int id in foundedSubjectGroupIds)
            {
                bool isExisted = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == id)).Count() == listOfSubjectId.Count;
                if (isExisted)
                {
                    return null;
                }
            }
            Models.SubjectGroup insertSubjectGroupModels = new Models.SubjectGroup
            {
                GroupCode = createSubjectGroupParam.GroupCode,
                Status = Consts.STATUS_ACTIVE
            };
             _uow.SubjectGroupRepository.Insert(insertSubjectGroupModels);
            int result =  await _uow.CommitAsync();
            if (result <= 0)
            {
                return null;
            }
            foreach (int subjectId in listOfSubjectId)
            {
                Models.SubjectGroupDetail insertSubjectGroupDetailModel = new SubjectGroupDetail
                {
                    SubjectGroupId = insertSubjectGroupModels.Id,
                    SubjectId = subjectId,
                };
                _uow.SubjecGroupDetailRepository.Insert(insertSubjectGroupDetailModel);
                result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    return null;
                }
            }
            List<Models.Subject> subjects = (await _uow.SubjectRepository.Get(filter: s => listOfSubjectId.Contains(s.Id))).ToList();
            List<SubjectDataSet> subjectDatas = new List<SubjectDataSet>();
            foreach (Models.Subject subject in subjects)
            {
                subjectDatas.Add(_mapper.Map<SubjectDataSet>(subject));
            }
            CreateSubjectGroupDataset createSubjectGroupDataset = new CreateSubjectGroupDataset
            {
                Id = insertSubjectGroupModels.Id,
                GroupCode = createSubjectGroupParam.GroupCode,
                ListOfSubject = subjectDatas,
                Status = insertSubjectGroupModels.Status
            };
            return createSubjectGroupDataset;

        }

        public async Task<CreateSubjectGroupDataset> UpdateSubjectGroup(UpdateSubjectGroupParam updateSubjectGroupParam)
        {
            int id = updateSubjectGroupParam.Id;
            List<int> listOfSubjectId = updateSubjectGroupParam.ListOfSubjectId;
            if( updateSubjectGroupParam.Id < 1)
            {
                return null;
            }
            if (updateSubjectGroupParam.ListOfSubjectId.Count != updateSubjectGroupParam.ListOfSubjectId.Distinct().Count())
            {
                return null;
            }
            if (listOfSubjectId == null || listOfSubjectId.Count < Consts.REQUIRED_NUMBER_SUBJECTS)
            {
                return null;
            }
            if (updateSubjectGroupParam.GroupCode == null || updateSubjectGroupParam.GroupCode.Equals(""))
            {
                return null;
            }
            Models.SubjectGroup existSubjectGroupByCode = await _uow.SubjectGroupRepository.GetFirst(filter: e => e.GroupCode.Equals(updateSubjectGroupParam.GroupCode));
            if (existSubjectGroupByCode != null && existSubjectGroupByCode.Id != updateSubjectGroupParam.Id)
            {
                return null;
            }

            IEnumerable<int> foundedSubjectGroupIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => listOfSubjectId.Contains(s.SubjectId)))
                .GroupBy(s => s.SubjectGroupId).Where(g => g.Count() == listOfSubjectId.Count()).Select(g => g.Key);
            foreach (int aid in foundedSubjectGroupIds)
            {
                bool isExisted = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == aid)).Count() == listOfSubjectId.Count;
                if (isExisted && aid != updateSubjectGroupParam.Id)
                {
                    return null;
                }
            }

            Models.SubjectGroup updateSubjectGroupModel = await _uow.SubjectGroupRepository.GetById(id);

            updateSubjectGroupModel.GroupCode = updateSubjectGroupParam.GroupCode;
            updateSubjectGroupModel.Status = updateSubjectGroupParam.Status;

             _uow.SubjectGroupRepository.Update(updateSubjectGroupModel);
            int result = await _uow.CommitAsync();
            if (result <= 0)
            {
                return null;
            }

            IEnumerable<int> oldSubjectIds = (await _uow.SubjecGroupDetailRepository.Get(filter: s => s.SubjectGroupId == updateSubjectGroupModel.Id))
                .Select(s => s.SubjectId);
            foreach (int oldSubjectId in oldSubjectIds)
            {
                _uow.SubjecGroupDetailRepository.DeleteComposite(filter: s => s.SubjectId == oldSubjectId && s.SubjectGroupId == updateSubjectGroupModel.Id);
            }
           
            await _uow.CommitAsync();

            foreach (int subjectId in listOfSubjectId)
            {
                Models.SubjectGroupDetail insertSubjectGroupDetailModel = new SubjectGroupDetail
                {
                    SubjectGroupId = id,
                    SubjectId = subjectId,
                };
                _uow.SubjecGroupDetailRepository.Insert(insertSubjectGroupDetailModel);
                result = await _uow.CommitAsync();
                if (result <= 0)
                {
                    return null;
                }
            }
            List<Models.Subject> subjects = (await _uow.SubjectRepository.Get(filter: s => listOfSubjectId.Contains(s.Id))).ToList();
            List<SubjectDataSet> subjectDatas = new List<SubjectDataSet>();
            foreach (Models.Subject subject in subjects)
            {
                subjectDatas.Add(_mapper.Map<SubjectDataSet>(subject));
            }
            CreateSubjectGroupDataset updateSubjectGroupDataset = new CreateSubjectGroupDataset
            {
                Id = updateSubjectGroupParam.Id,
                GroupCode = updateSubjectGroupParam.GroupCode,
                ListOfSubject = subjectDatas,
                Status = updateSubjectGroupParam.Status
            };
            return updateSubjectGroupDataset;
        }
    }
}
