using AutoMapper;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.University
{
    public class UniversityService : IUniversityService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public UniversityService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UniversityDataSet>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam)
        {
            List<MajorDetail> majorDetails = (await _uow.MajorDetailRepository.Get(filter: w => w.MajorId == universityParam.MajorId, includeProperties: "University")).ToList();
            if (majorDetails == null || !majorDetails.Any())
            {
                return null;
            }
            foreach(MajorDetail majorDetail in majorDetails)
            {
                EntryMark entryMark = await _uow.EntryMarkRepository.GetFirst(filter: e => e.SubjectGroupId == universityParam.SubjectGroupId);
                if (entryMark.Mark > universityParam.TotalMark)
                {
                    majorDetails.Remove(majorDetail);
                }
            }
            return majorDetails.Select(s => _mapper.Map<UniversityDataSet>(s.University));
        }

        public async Task<IEnumerable<Models.University>> GetUniversities()
        {
            IEnumerable<Models.University> universities = await _uow.UniversityRepository.Get();
            return universities;
        }

        public async Task<DetailUniversityDataSet> GetDetailUniversity(int universityId)
        {
            Models.University university = await _uow.UniversityRepository.GetFirst(filter: u => u.Id == universityId,
                                            includeProperties: "MajorDetails");
            DetailUniversityDataSet universityDataSet = _mapper.Map<DetailUniversityDataSet>(university);
            List<Major> majors = new List<Major>();
            List<UniMajorDataSet> uniMajorDataSets;
            foreach (MajorDetail majorDetail in university.MajorDetails)
            {
                Major major = await _uow.MajorRepository.GetById(majorDetail.MajorId);
                majors.Add(major);
            }

            //Get list majors
            uniMajorDataSets = majors.Select(m => _mapper.Map<UniMajorDataSet>(m)).ToList();

            foreach(UniMajorDataSet uniMajorDataSet in uniMajorDataSets)
            {
                MajorDetail majorDetail = await _uow.MajorDetailRepository.GetFirst(
                                                filter: m => m.MajorId == uniMajorDataSet.Id && m.UniversityId == universityDataSet.Id);
                List<int> subjectGroupIds = (await _uow.EntryMarkRepository.Get(
                                                filter: e => e.MajorDetailId == majorDetail.Id, includeProperties: "SubjectGroup"))
                                                .Select(e => e.SubjectGroupId).Distinct().ToList();
                List<UniSubjectGroupDataSet> uniSubjectGroupDataSets = new List<UniSubjectGroupDataSet>();
                subjectGroupIds.ForEach(async s =>
                {
                    uniSubjectGroupDataSets.Add(_mapper.Map<UniSubjectGroupDataSet>(await _uow.SubjectGroupRepository.GetById(s)));
                });

                foreach(UniSubjectGroupDataSet uniSubjectGroupDataSet in uniSubjectGroupDataSets)
                {
                    List<UniEntryMarkDataSet> entryMarks = (await _uow.EntryMarkRepository.Get(
                                                    filter: e => e.SubjectGroupId == uniSubjectGroupDataSet.Id && e.MajorDetailId == majorDetail.Id ))
                                                    .Select(e => _mapper.Map<UniEntryMarkDataSet>(e)).ToList();
                    uniSubjectGroupDataSet.EntryMarks = entryMarks;
                }
                uniMajorDataSet.SubjectGroups = uniSubjectGroupDataSets;
            }
            universityDataSet.Majors = uniMajorDataSets;
            return universityDataSet;
        }
    }
}
