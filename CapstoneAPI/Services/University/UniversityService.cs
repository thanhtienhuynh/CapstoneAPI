using AutoMapper;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Helpers;

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

        public async Task<IEnumerable<UniversityDataSetBaseOnTrainingProgram>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam)
        {
            //Lấy ra tất cả các trường có ngành đã chọn
            List<MajorDetail> majorDetails = (await _uow.MajorDetailRepository.Get(filter: w => w.MajorId == universityParam.MajorId, includeProperties: "University,TrainingProgram,AdmissionCriteria")).ToList();
            if (majorDetails == null || !majorDetails.Any())
            {
                return null;
            }

            List<UniversityDataSetBaseOnTrainingProgram> universityDataSetsBaseOnTrainingProgram = new List<UniversityDataSetBaseOnTrainingProgram>();

            IEnumerable<IGrouping<int, MajorDetail>> majorDetailsBasedOnTrainingProgram = majorDetails
                                                                                            .GroupBy(m => m.TrainingProgramId);
            foreach (IGrouping<int, MajorDetail> majorDetailBasedOnTrainingProgram in majorDetailsBasedOnTrainingProgram)
            {
                List<UniversityDataSet> universityDataSets = new List<UniversityDataSet>();
                foreach (MajorDetail majorDetail in majorDetailBasedOnTrainingProgram.ToList())
                {
                    EntryMark entryMark = await _uow.EntryMarkRepository.
                        GetFirst(filter: e => e.SubjectGroupId == universityParam.SubjectGroupId 
                                        && e.MajorDetailId == majorDetail.Id 
                                        && e.Mark > 0
                                        && e.Year == Consts.NEAREST_YEAR);
                    if (entryMark == null)
                    {
                        majorDetails.Remove(majorDetail);
                        continue;
                    }

                    if (entryMark.Mark > universityParam.TotalMark)
                    {
                        majorDetails.Remove(majorDetail);
                        continue;
                    }
                    else
                    {
                        UniversityDataSet universityDataSet = _mapper.Map<UniversityDataSet>(majorDetail.University);
                        universityDataSet.NearestYearEntryMark = (double)entryMark.Mark;
                        universityDataSet.NumberOfStudents = majorDetail.AdmissionCriteria
                                    .FirstOrDefault(a => a.Year == 2020) != null ? 
                                    majorDetail.AdmissionCriteria.FirstOrDefault(a => a.Year == 2020).Quantity : null;
                        List<int> majorCaringUserIds = (await _uow.UserMajorRepository.Get(u => u.MajorId == universityParam.MajorId)).Select(m => m.UserId).ToList();
                        List<int> universityCaringUserIds = (await _uow.UserUniversityRepository.Get(u => u.UniversityId == universityDataSet.Id)).Select(m => m.UserId).ToList();
                        universityDataSet.NumberOfCaring = majorCaringUserIds.Intersect(universityCaringUserIds).Count();
                        universityDataSets.Add(universityDataSet);
                    }
                }
                if (universityDataSets.Any())
                {
                    universityDataSetsBaseOnTrainingProgram.Add(new UniversityDataSetBaseOnTrainingProgram()
                    {
                        Id = majorDetailBasedOnTrainingProgram.Key,
                        Name = (await _uow.TrainingProgramRepository.GetById(majorDetailBasedOnTrainingProgram.Key)).Name,
                        Universities = universityDataSets.OrderByDescending(u => u.NearestYearEntryMark).ToList()
                    });
                }
            }
            
            return universityDataSetsBaseOnTrainingProgram;
        }

        public async Task<IEnumerable<AdminUniversityDataSet>> GetUniversities()
        {
            IEnumerable<AdminUniversityDataSet> universities = (await _uow.UniversityRepository.Get()).OrderBy(s => s.UpdatedDate)
                                                        .Select(u => _mapper.Map<AdminUniversityDataSet>(u));
            return universities;
        }

        public async Task<DetailUniversityDataSet> GetDetailUniversity(int universityId)
        {
            Models.University university = await _uow.UniversityRepository.GetFirst(filter: u => u.Id == universityId,
                                            includeProperties: "MajorDetails");
            DetailUniversityDataSet universityDataSet = _mapper.Map<DetailUniversityDataSet>(university);
            List<UniMajorDataSet> uniMajorDataSets = new List<UniMajorDataSet>();
            foreach (MajorDetail majorDetail in university.MajorDetails)
            {
                Models.Major major = await _uow.MajorRepository.GetById(majorDetail.MajorId);
                AdmissionCriterion admissionCriterion = await _uow.AdmissionCriterionRepository
                                                                .GetFirst(a => a.Year == 2021 && a.MajorDetailId == majorDetail.Id);
                UniMajorDataSet uniMajorDataSet = _mapper.Map<UniMajorDataSet>(major);
                uniMajorDataSet.NumberOfStudents = admissionCriterion != null ? admissionCriterion.Quantity : null;
                uniMajorDataSet.Code = majorDetail.MajorCode;
                uniMajorDataSet.TrainingProgramId = majorDetail.TrainingProgramId;
                uniMajorDataSet.TrainingProgramName = (await _uow.TrainingProgramRepository.GetById(majorDetail.TrainingProgramId)).Name;
                uniMajorDataSets.Add(uniMajorDataSet);
            }

            foreach(UniMajorDataSet uniMajorDataSet in uniMajorDataSets)
            {
                MajorDetail majorDetail = await _uow.MajorDetailRepository.GetFirst(
                                                filter: m => m.MajorId == uniMajorDataSet.Id 
                                                && m.UniversityId == universityDataSet.Id
                                                && m.TrainingProgramId == uniMajorDataSet.TrainingProgramId);
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
                                                    filter: e => e.SubjectGroupId == uniSubjectGroupDataSet.Id && e.MajorDetailId == majorDetail.Id && (e.Year == Consts.YEAR_2019 || e.Year == Consts.YEAR_2020)))
                                                    .Select(e => _mapper.Map<UniEntryMarkDataSet>(e)).OrderBy(e => e.Year).ToList();
                    uniSubjectGroupDataSet.EntryMarks = entryMarks;
                }
                uniSubjectGroupDataSets = uniSubjectGroupDataSets.Where(s => s.EntryMarks.Any()).ToList();
                uniMajorDataSet.SubjectGroups = uniSubjectGroupDataSets;
            }
            universityDataSet.Majors = uniMajorDataSets;
            return universityDataSet;  
        }

        public async Task<AdminUniversityDataSet> CreateNewAnUniversity(CreateUniversityDataset createUniversityDataset)
        {
            if (createUniversityDataset.Name.Equals("") || createUniversityDataset.Code.Equals("") || (createUniversityDataset.Status != 0 && createUniversityDataset.Status != Consts.STATUS_ACTIVE))
                return null;
            Models.University ExistUni = await _uow.UniversityRepository.GetFirst(filter: u => u.Code.Equals(createUniversityDataset.Code));
            if (ExistUni != null)
            {
                return null;
            }
            Models.University university = _mapper.Map<Models.University>(createUniversityDataset);
            _uow.UniversityRepository.Insert(university);
            int result = await _uow.CommitAsync();
            if (result > 0)
            {
                return _mapper.Map<AdminUniversityDataSet>(university);
            }
            return null;
        }

        public async Task<AdminUniversityDataSet> UpdateUniversity(AdminUniversityDataSet adminUniversityDataSet)
        {
            if (adminUniversityDataSet.Name.Equals("") || adminUniversityDataSet.Code.Equals("") || (adminUniversityDataSet.Status != Consts.STATUS_ACTIVE && adminUniversityDataSet.Status != Consts.STATUS_INACTIVE))
                return null;
            Models.University existUni = await _uow.UniversityRepository.GetFirst(filter: u => u.Code.Equals(adminUniversityDataSet.Code));
            if (existUni != null && existUni.Id != adminUniversityDataSet.Id)
            {
                return null;
            }
            Models.University updatedUni = await _uow.UniversityRepository.GetById(adminUniversityDataSet.Id);
            if (updatedUni == null)
            {
                return null;
            }
            updatedUni.Code = adminUniversityDataSet.Code;
            updatedUni.Name = adminUniversityDataSet.Name;
            updatedUni.Address = adminUniversityDataSet.Address;
            //updatedUni.LogoUrl = adminUniversityDataSet.LogoUrl;
            updatedUni.Description = adminUniversityDataSet.Description;
            updatedUni.Phone = adminUniversityDataSet.Phone;
            updatedUni.WebUrl = adminUniversityDataSet.WebUrl;
            updatedUni.TuitionType = adminUniversityDataSet.TuitionType;
            updatedUni.TuitionFrom = adminUniversityDataSet.TuitionFrom;
            updatedUni.TuitionTo = adminUniversityDataSet.TuitionTo;
            updatedUni.Rating = adminUniversityDataSet.Rating;
            updatedUni.Status = adminUniversityDataSet.Status;

            _uow.UniversityRepository.Update(updatedUni);
            int result = await _uow.CommitAsync();
            if (result > 0)
            {
                return _mapper.Map<AdminUniversityDataSet>(updatedUni);
            }
            return null;
        }

        public async Task<DetailUniversityDataSet> AddMajorToUniversity(AddingMajorUniversityParam addingMajorUniversityParam)
        {
            MajorDetail majorDetail = null;

            MajorDetail existedMajorDetail = await _uow.MajorDetailRepository
                       .GetFirst(m => m.MajorId == addingMajorUniversityParam.MajorId
                       && m.UniversityId == addingMajorUniversityParam.UniversityId
                       && m.TrainingProgramId == addingMajorUniversityParam.TrainingProgramId);
            if (existedMajorDetail != null)
            {
                return null;
            }
            majorDetail = new MajorDetail()
            {
                MajorId = addingMajorUniversityParam.MajorId,
                UniversityId = addingMajorUniversityParam.UniversityId,
                TrainingProgramId = addingMajorUniversityParam.TrainingProgramId,
                MajorCode = addingMajorUniversityParam.MajorCode,
            };

            _uow.MajorDetailRepository.Insert(majorDetail);

            if ((await _uow.CommitAsync()) <= 0)
            {
                return null;
            }

            AdmissionCriterion admissionCriterion = new AdmissionCriterion()
            {
                MajorDetailId = majorDetail.Id,
                Quantity = addingMajorUniversityParam.NumberOfStudents,
                Year = 2021
            };

            _uow.AdmissionCriterionRepository.Insert(admissionCriterion);

            if ((await _uow.CommitAsync()) <= 0)
            {
                return null;
            }

            if (addingMajorUniversityParam.SubjectGroups != null && addingMajorUniversityParam.SubjectGroups.Any())
            {
                foreach (UniSubjectGroupDataSet uniSubjectGroupDataSet in addingMajorUniversityParam.SubjectGroups)
                {
                    if (uniSubjectGroupDataSet.EntryMarks == null || !uniSubjectGroupDataSet.EntryMarks.Any())
                    {
                        _uow.EntryMarkRepository.Insert(new EntryMark()
                            {
                                MajorDetailId = majorDetail.Id,
                                Mark = null,
                                Year = Consts.YEAR_2020,
                                SubjectGroupId = uniSubjectGroupDataSet.Id
                            }
                        );
                    }
                    else
                    {
                        foreach (UniEntryMarkDataSet uniEntryMarkDataSet in uniSubjectGroupDataSet.EntryMarks)
                        {
                            _uow.EntryMarkRepository.Insert(new EntryMark()
                                {
                                    MajorDetailId = majorDetail.Id,
                                    Mark = uniEntryMarkDataSet.Mark,
                                    Year = uniEntryMarkDataSet.Year,
                                    SubjectGroupId = uniSubjectGroupDataSet.Id
                                }
                            );
                        }
                    }

                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        return null;
                    }
                }
            }
            return await GetDetailUniversity(addingMajorUniversityParam.UniversityId);
        }
        public async Task<DetailUniversityDataSet> UpdateMajorOfUniversity(UpdatingMajorUniversityParam updatingMajorUniversityParam)
        {
            if (updatingMajorUniversityParam.MajorId < 0)
            {
                return null;
            }

            bool isMajorExisted = (await _uow.MajorRepository.GetById(updatingMajorUniversityParam.MajorId)) != null;

            if (!isMajorExisted)
            {
                return null;
            }

            MajorDetail majorDetail = await _uow.MajorDetailRepository
                .GetFirst(filter: m => m.MajorId == updatingMajorUniversityParam.MajorId 
                && m.UniversityId == updatingMajorUniversityParam.UniversityId 
                && m.TrainingProgramId == updatingMajorUniversityParam.OldTrainingProgramId);
            if (majorDetail == null)
            {
                return null;
            }

            if (updatingMajorUniversityParam.OldTrainingProgramId != updatingMajorUniversityParam.NewTrainingProgramId)
            {
                MajorDetail exitedUpdateMajorDetail = await _uow.MajorDetailRepository
                .GetFirst(filter: m => m.MajorId == updatingMajorUniversityParam.MajorId
                && m.UniversityId == updatingMajorUniversityParam.UniversityId
                && m.TrainingProgramId == updatingMajorUniversityParam.NewTrainingProgramId);

                if (exitedUpdateMajorDetail != null)
                {
                    return null;
                }
            }

            majorDetail.TrainingProgramId = updatingMajorUniversityParam.NewTrainingProgramId;
            majorDetail.MajorCode = updatingMajorUniversityParam.MajorCode;
            _uow.MajorDetailRepository.Update(majorDetail);

            AdmissionCriterion admissionCriterion = (await _uow.AdmissionCriterionRepository
                .Get(filter: a => a.MajorDetailId == majorDetail.Id && a.Year == 2021)).FirstOrDefault();

            if (admissionCriterion != null)
            {
                admissionCriterion.Quantity = updatingMajorUniversityParam.NumberOfStudents;
                _uow.AdmissionCriterionRepository.Update(admissionCriterion);
            } else
            {
                admissionCriterion = new AdmissionCriterion()
                {
                    MajorDetailId = majorDetail.Id,
                    Year = 2021,
                    Quantity = updatingMajorUniversityParam.NumberOfStudents
                };
                _uow.AdmissionCriterionRepository.Insert(admissionCriterion);
            }

            if ((await _uow.CommitAsync()) <= 0)
            {
                return null;
            }

            foreach (UpdatingUniSubjectGroupDataSet updatingUniSubjectGroupDataSet in updatingMajorUniversityParam.SubjectGroups)
            {
               if (updatingUniSubjectGroupDataSet.IsDeleted)
               {
                    foreach (UniEntryMarkDataSet entryMark in updatingUniSubjectGroupDataSet.EntryMarks)
                    {
                        _uow.EntryMarkRepository.Delete(entryMark.Id);
                    }
               } else
               {
                    foreach (UniEntryMarkDataSet entryMark in updatingUniSubjectGroupDataSet.EntryMarks)
                    {
                        if (entryMark.Id < 0)
                        {
                            if (entryMark.Mark <= 0)
                            {
                                return null;
                            }
                            EntryMark existedEntryMark = await _uow.EntryMarkRepository
                                .GetFirst(filter: s => s.SubjectGroupId == updatingUniSubjectGroupDataSet.Id
                                                && s.MajorDetailId == majorDetail.Id && s.Year == entryMark.Year);
                            if (existedEntryMark != null)
                            {
                                return null;
                            }
                            EntryMark newEntryMark = new EntryMark()
                            {
                                MajorDetailId = majorDetail.Id,
                                Mark = entryMark.Mark,
                                SubjectGroupId = updatingUniSubjectGroupDataSet.Id,
                                Year = entryMark.Year
                            };
                            _uow.EntryMarkRepository.Insert(newEntryMark);
                        } else
                        {
                            EntryMark existedEntryMark = await _uow.EntryMarkRepository.GetById(entryMark.Id);
                            existedEntryMark.Mark = entryMark.Mark;
                            _uow.EntryMarkRepository.Update(existedEntryMark);
                        }
                    }
                }
               
            }

            if ((await _uow.CommitAsync()) <= 0)
            {
                return null;
            }
            
            return await GetDetailUniversity(updatingMajorUniversityParam.UniversityId);
        }
    }

}
