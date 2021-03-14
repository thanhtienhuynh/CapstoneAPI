using AutoMapper;
using CapstoneAPI.DataSets.Major;
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
                subjectGroupDataSets.Add(new SubjectGroupDataSet { TotalMark = totalMark, Name = subjectGroup.GroupCode, Id = subjectGroup.Id });
            }

            if (!subjectGroupParam.IsSuggest)
            {
                return subjectGroupDataSets.OrderByDescending(o => o.TotalMark).ToList();
            }

            IEnumerable<SubjectGroupDataSet> suggestedSubjectGroups = subjectGroupDataSets.OrderByDescending(o => o.TotalMark).Take(Consts.NUMBER_OF_SUGGESTED_GROUP).ToList();

            foreach (SubjectGroupDataSet suggestGroup in suggestedSubjectGroups)
            {
                List<int> majorIds = (await _uow.WeightNumberRepository.
                                                            Get(filter: weightNumbers => weightNumbers.SubjectGroupId == suggestGroup.Id))
                                                            .Select(w => w.MajorId).Distinct().ToList();
                //Lọc những id ngành không có trường phù hợp vì thấp hơn điểm chuẩn
                await FilterMajorsWithEntryMark(majorIds, suggestGroup);

                //Tính trọng số từng ngành
                
                suggestGroup.SuggestedMajors = (await GenerateListMajors(subjectGroupParam, suggestGroup, majorIds)).OrderByDescending(o => o.WeightMark).Take(5).ToList();
            }

            return suggestedSubjectGroups;
        }

        private async Task FilterMajorsWithEntryMark(List<int> majorIds, SubjectGroupDataSet subjectGroup)
        {
            List<MajorDataSet> filteredMajors = new List<MajorDataSet>();
            foreach (int majorId in majorIds.ToList())
            {
                if (majorId == 193)
                {
                    Console.WriteLine();
                }
                List<int> majorDetailIds = (await _uow.MajorDetailRepository.Get(filter: m => m.MajorId == majorId)).Select(m => m.Id).ToList();
                bool isSuitable = (await _uow.EntryMarkRepository.Get(filter: (e => majorDetailIds.Contains(e.MajorDetailId) && e.Mark <= subjectGroup.TotalMark))).Any();
                if (!isSuitable)
                {
                    majorIds.Remove(majorId);
                }
            }
        }

        private async Task<List<MajorDataSet>> GenerateListMajors(SubjectGroupParam subjectGroupParam, SubjectGroupDataSet suggestGroup, List<int> majorIds)
        {
            List<MajorDataSet> majorDataSets = new List<MajorDataSet>();
            foreach (int majorId in majorIds)
            {
                IEnumerable<WeightNumber> weightNumbers = await _uow.WeightNumberRepository.Get(w => w.MajorId == majorId && w.SubjectGroupId == suggestGroup.Id);
                MajorDataSet major = _mapper.Map<MajorDataSet>(await _uow.MajorRepository.GetById(majorId));
                major.WeightMark = CalculateTotalWeightMark(subjectGroupParam, weightNumbers);
                majorDataSets.Add(major);
            }
            return majorDataSets;
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

            return totalMark / totalWeight;
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

        private double CalculateSubjectGroupMark(SubjectGroupParam subjectGroupParam, List<SubjectGroupDetail> subjectGroupDetails)
        {
            double totalMark = 0;
            if (subjectGroupDetails.Count > 3)
            {
                return 0;
            }
            foreach(SubjectGroupDetail subjectGroupDetail in subjectGroupDetails)
            {
                MarkParam markParam =  subjectGroupParam.Marks.FirstOrDefault(m => m.SubjectId == subjectGroupDetail.SubjectId);
                if (markParam != null)
                {
                    totalMark += markParam.Mark;
                }
            } 
            return totalMark;
        }
    }
}
