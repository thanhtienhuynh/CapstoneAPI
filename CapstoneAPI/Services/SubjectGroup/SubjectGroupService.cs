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
                double totalMark = CalculateTotalMark(subjectGroupParam, subjectGroup);
                subjectGroupDataSets.Add(new SubjectGroupDataSet { TotalMark = totalMark, Name = subjectGroup.GroupCode, Id = subjectGroup.Id });
            }

            if (!subjectGroupParam.IsSuggest)
            {
                return subjectGroupDataSets.OrderByDescending(o => o.TotalMark).ToList();
            }

            IEnumerable<SubjectGroupDataSet> suggestedSubjectGroups = subjectGroupDataSets.OrderByDescending(o => o.TotalMark).Take(Consts.NUMBER_OF_SUGGESTED_GROUP).ToList();

            foreach (SubjectGroupDataSet suggestGroup in suggestedSubjectGroups)
            {
                IEnumerable<WeightNumber> weightNumbers = await _uow.WeightNumberRepository.Get(filter: weightNumbers => weightNumbers.SubjectGroupId == suggestGroup.Id, includeProperties: "Major");
                List<MajorDataSet> majors = weightNumbers.Select(w => _mapper.Map<MajorDataSet>(w.Major)).ToList();
                //Lọc những ngành không có trường phù hợp vì thấp hơn điểm chuẩn
                await FilterMajorsWithEntryMark(majors, suggestGroup, weightNumbers);
                CalculateWeightMark(subjectGroupParam, suggestGroup, weightNumbers, majors);
                suggestGroup.SuggestedMajors = majors.OrderByDescending(o => o.WeightMark).Take(3).ToList();
            }

            return suggestedSubjectGroups;
        }

        private async Task FilterMajorsWithEntryMark(List<MajorDataSet> majors, SubjectGroupDataSet subjectGroup, IEnumerable<WeightNumber> weightNumbers)
        {
            List<MajorDataSet> filteredMajors = new List<MajorDataSet>();
            foreach (MajorDataSet majorDataSet in majors.ToList())
            {
                WeightNumber weightNumber = weightNumbers.Where(w => w.MajorId == majorDataSet.Id).First();
                bool isSuitable = (await _uow.EntryMarkRepository.Get(filter: (e => e.WeightNumberId == weightNumber.Id && e.Mark <= subjectGroup.TotalMark))).Any();
                if (!isSuitable)
                {
                    majors.Remove(majorDataSet);
                }
            }
        }

        private void CalculateWeightMark(SubjectGroupParam subjectGroupParam, SubjectGroupDataSet suggestGroup, IEnumerable<WeightNumber> weightNumbers, List<MajorDataSet> majors)
        {
            foreach (MajorDataSet majorDataSet in majors)
            {
                WeightNumber weightNumber = weightNumbers.Where(w => w.MajorId == majorDataSet.Id).First();
                majorDataSet.WeightMark = suggestGroup.TotalMark + CalculateMark(subjectGroupParam, weightNumber.SubjectId, (weightNumber.Weight - 1));
            }
        }

        private double CalculateTotalMark(SubjectGroupParam subjectGroupParam, Models.SubjectGroup subjectGroup)
        {
            double totalMark = 0;
            foreach (SubjectGroupDetail subjectGroupDetail in subjectGroup.SubjectGroupDetails)
            {
                totalMark += CalculateMark(subjectGroupParam, subjectGroupDetail.SubjectId, Consts.DEFAULT_WEIGHT_NUMBER);
            }

            return totalMark;
        }

        private double CalculateMark(SubjectGroupParam subjectGroupParam, int? id, int? weight)
        {
            if (id == null)
            {
                return 0;
            }

            if (weight == null)
            {
                return 0;
            }

            foreach (MarkParam markParam in subjectGroupParam.Marks)
            {
                if (markParam.SubjectId == id)
                {
                    return (double)(markParam.Mark * weight);
                }
            }
            return 0;
        }
    }
}
