using AutoMapper;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CapstoneAPI.Helpers;
using Microsoft.AspNetCore.Http;
using System.IO;
using Firebase.Auth;
using System.Threading;
using Firebase.Storage;
using CapstoneAPI.Wrappers;

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

        public async Task<Response<IEnumerable<TrainingProgramBasedUniversityDataSet>>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam, string token)
        {
            Response<IEnumerable<TrainingProgramBasedUniversityDataSet>> response = new Response<IEnumerable<TrainingProgramBasedUniversityDataSet>>();
            int userId = 0;
            if (token != null && token.Trim().Length > 0)
            {
                string userIdString = JWTUtils.GetUserIdFromJwtToken(token);
                if (userIdString != null && userIdString.Length > 0)
                {
                    userId = Int32.Parse(userIdString);
                }
            }

            List<TrainingProgramBasedUniversityDataSet> trainingProgramBasedUniversityDataSets = new List<TrainingProgramBasedUniversityDataSet>();

            Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
            Season previousSeason = await _uow.SeasonRepository.GetPreviousSeason();

            //Lấy ra tất cả các trường va hệ có ngành đã chọn
            List<MajorDetail> majorDetails = (await _uow.MajorDetailRepository
                .Get(filter: w => w.MajorId == universityParam.MajorId, 
                    includeProperties: "University,TrainingProgram,AdmissionCriterion,AdmissionCriterion.SubAdmissionCriteria"))
                .ToList();
            if (majorDetails == null || !majorDetails.Any())
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Hiện tại không có trường nào dạy ngành này!");
                return response;
            }

            var groupsByUnis = majorDetails
                .GroupBy(m => m.University);

            List<MajorDetail> validMajorDetails = new List<MajorDetail>();

            foreach (var groupsByUni in groupsByUnis)
            {
                TrainingProgramBasedUniversityDataSet trainingProgramBasedUniversityDataSet = new TrainingProgramBasedUniversityDataSet();
                trainingProgramBasedUniversityDataSet = _mapper.Map<TrainingProgramBasedUniversityDataSet>(groupsByUni.Key);
                var groupByTrainingPrograms = groupsByUni.GroupBy(m => m.TrainingProgram);
                List<TrainingProgramDataSet> trainingProgramDataSets = new List<TrainingProgramDataSet>();
                foreach (var groupByTrainingProgram in groupByTrainingPrograms)
                {
                    TrainingProgramDataSet trainingProgramDataSet = new TrainingProgramDataSet();
                    List<SeasonDataSet> seasonDataSets = new List<SeasonDataSet>();
                    MajorDetail currentMajorDetail = groupByTrainingProgram.Where(m => m.SeasonId == currentSeason.Id).FirstOrDefault();
                    MajorDetail previousMajorDetail = groupByTrainingProgram.Where(m => m.SeasonId == previousSeason.Id).FirstOrDefault();
                    if (currentMajorDetail == null || previousMajorDetail == null)
                    {
                        continue;
                    }

                    SeasonDataSet currentSeasonDataSet = new SeasonDataSet
                    {
                        Id = currentSeason.Id,
                        Name = currentSeason.Name
                    };
                    SeasonDataSet previousSeasonDataSet = new SeasonDataSet
                    {
                        Id = previousSeason.Id,
                        Name = previousSeason.Name
                    };

                    trainingProgramDataSet.Id = groupByTrainingProgram.Key.Id;
                    trainingProgramDataSet.Name = groupByTrainingProgram.Key.Name;

                    if (currentMajorDetail.AdmissionCriterion == null || previousMajorDetail.AdmissionCriterion == null)
                    {
                        continue;
                    }

                    if (currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria == null
                        || !currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Any()
                        || previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria == null
                        || !previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Any())
                    {
                        continue;
                    }

                    List<SubAdmissionCriterion> currentSubAdmissionCriterias = currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                        .Where(a => a.AdmissionMethodId == 1 && (a.Gender == universityParam.Gender || a.Gender == null)
                         && (a.ProvinceId == universityParam.ProvinceId || a.ProvinceId == null)).ToList();
                    List<SubAdmissionCriterion> previousSubAdmissionCriterias = previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                        .Where(a => a.AdmissionMethodId == 1 && (a.Gender == universityParam.Gender || a.Gender == null)
                         && (a.ProvinceId == universityParam.ProvinceId || a.ProvinceId == null)).ToList();

                    if (!currentSubAdmissionCriterias.Any() || !previousSubAdmissionCriterias.Any())
                    {
                        continue;
                    }

                    EntryMark currentEntryMark = null;
                    EntryMark previousEntryMark = null;

                    foreach (SubAdmissionCriterion currentSubAdmissionCriteria in currentSubAdmissionCriterias)
                    {
                        currentEntryMark = (await _uow.EntryMarkRepository
                            .Get(filter: e => e.SubAdmissionCriterionId == currentSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
                                includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup,SubAdmissionCriterion,FollowingDetails"))
                                .Where(e => e.MajorSubjectGroup.SubjectGroupId == universityParam.SubjectGroupId
                                            && e.MajorSubjectGroup.MajorId == universityParam.MajorId).FirstOrDefault();
                        if (currentEntryMark != null)
                        {
                            break;
                        }
                    }

                    foreach (SubAdmissionCriterion previousSubAdmissionCriteria in previousSubAdmissionCriterias)
                    {
                        previousEntryMark = (await _uow.EntryMarkRepository
                            .Get(filter: e => e.SubAdmissionCriterionId == previousSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
                                includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup,SubAdmissionCriterion"))
                                .Where(e => e.MajorSubjectGroup.SubjectGroupId == universityParam.SubjectGroupId
                                            && e.MajorSubjectGroup.MajorId == universityParam.MajorId).FirstOrDefault();
                    }

                    if (currentEntryMark == null || previousEntryMark == null)
                    {
                        continue;
                    }

                    previousSeasonDataSet.EntryMark = previousEntryMark.Mark;
                    previousSeasonDataSet.NumberOfStudents = previousEntryMark.SubAdmissionCriterion.Quantity;
                    currentSeasonDataSet.NumberOfStudents = currentEntryMark.SubAdmissionCriterion.Quantity;
                    List<int> currentEntryMarkIds = (await _uow.EntryMarkRepository
                                                    .Get(filter: e => e.SubAdmissionCriterionId == currentEntryMark.SubAdmissionCriterionId))
                                                    .Select(e => e.Id).ToList();
                    seasonDataSets.Add(previousSeasonDataSet);
                    seasonDataSets.Add(currentSeasonDataSet);
                    trainingProgramDataSet.SeasonDataSets = seasonDataSets;
                    trainingProgramDataSet.NumberOfCaring = (await _uow.FollowingDetailRepository
                        .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId))).Count();
                    if (userId > 0)
                    {
                        trainingProgramDataSet.IsCared = (await _uow.FollowingDetailRepository.Get(filter: f => f.UserId == userId
                                                                                    && f.EntryMarkId == currentEntryMark.Id))
                                                                                    .Any();
                    }
                    IEnumerable<Models.Rank> ranks = (await _uow.FollowingDetailRepository
                                                            .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId),
                                                                includeProperties: "Rank"))
                                                            .Select(u => u.Rank).Where(r => r != null);
                    trainingProgramDataSet.Rank = _uow.RankRepository.CalculateRank(universityParam.TranscriptTypeId, universityParam.TotalMark, ranks);

                    trainingProgramDataSets.Add(trainingProgramDataSet);
                }
                trainingProgramBasedUniversityDataSet.TrainingProgramSets = trainingProgramDataSets;
                trainingProgramBasedUniversityDataSets.Add(trainingProgramBasedUniversityDataSet);
            }
  
            if (trainingProgramBasedUniversityDataSets.Any())
            {
                response.Succeeded = true;
                response.Data = trainingProgramBasedUniversityDataSets;
            } else
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Không có trường phù hợp!");
            }

            return response; ;
        }

        public async Task<Response<IEnumerable<AdminUniversityDataSet>>> GetUniversities()
        {
            Response<IEnumerable<AdminUniversityDataSet>> response = new Response<IEnumerable<AdminUniversityDataSet>>();
            IEnumerable<AdminUniversityDataSet> universities = (await _uow.UniversityRepository.Get()).OrderBy(s => s.UpdatedDate)
                                                        .Select(u => _mapper.Map<AdminUniversityDataSet>(u));
            if (universities == null || !universities.Any())
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Không có trường đại học nào!");
            } else
            {
                response.Succeeded = true;
                response.Data = universities;
            }
            return response;
        }

        public async Task<Response<DetailUniversityDataSet>> GetDetailUniversity(int universityId)
        {
            Response<DetailUniversityDataSet> response = new Response<DetailUniversityDataSet>();
        //    Models.University university = await _uow.UniversityRepository.GetFirst(filter: u => u.Id == universityId,
        //                                    includeProperties: "MajorDetails");
        //    if (university == null)
        //    {
        //        response.Succeeded = false;
        //        if (response.Errors == null)
        //        {
        //            response.Errors = new List<string>();
        //        }
        //        response.Errors.Add("Trường đại học này không tồn tại!");
        //        return response;
        //    }
        //    DetailUniversityDataSet universityDataSet = _mapper.Map<DetailUniversityDataSet>(university);
        //    List<UniMajorDataSet> uniMajorDataSets = new List<UniMajorDataSet>();
        //    foreach (MajorDetail majorDetail in university.MajorDetails)
        //    {
        //        Models.Major major = await _uow.MajorRepository.GetById(majorDetail.MajorId);
        //        AdmissionCriterion admissionCriterion = await _uow.AdmissionCriterionRepository
        //                                                        .GetFirst(a => a.Year == 2021 && a.MajorDetailId == majorDetail.Id);
        //        UniMajorDataSet uniMajorDataSet = _mapper.Map<UniMajorDataSet>(major);
        //        uniMajorDataSet.NumberOfStudents = admissionCriterion != null ? admissionCriterion.Quantity : null;
        //        uniMajorDataSet.Code = majorDetail.MajorCode;
        //        uniMajorDataSet.TrainingProgramId = majorDetail.TrainingProgramId;
        //        uniMajorDataSet.TrainingProgramName = (await _uow.TrainingProgramRepository.GetById(majorDetail.TrainingProgramId)).Name;
        //        uniMajorDataSets.Add(uniMajorDataSet);
        //    }

        //    foreach(UniMajorDataSet uniMajorDataSet in uniMajorDataSets)
        //    {
        //        MajorDetail majorDetail = await _uow.MajorDetailRepository.GetFirst(
        //                                        filter: m => m.MajorId == uniMajorDataSet.Id 
        //                                        && m.UniversityId == universityDataSet.Id
        //                                        && m.TrainingProgramId == uniMajorDataSet.TrainingProgramId);
        //        List<int> subjectGroupIds = (await _uow.EntryMarkRepository.Get(
        //                                        filter: e => e.MajorDetailId == majorDetail.Id, includeProperties: "SubjectGroup"))
        //                                        .Select(e => e.SubjectGroupId).Distinct().ToList();
        //        List<UniSubjectGroupDataSet> uniSubjectGroupDataSets = new List<UniSubjectGroupDataSet>();
        //        subjectGroupIds.ForEach(async s =>
        //        {
        //            uniSubjectGroupDataSets.Add(_mapper.Map<UniSubjectGroupDataSet>(await _uow.SubjectGroupRepository.GetById(s)));
        //        });

        //        foreach(UniSubjectGroupDataSet uniSubjectGroupDataSet in uniSubjectGroupDataSets)
        //        {
        //            List<UniEntryMarkDataSet> entryMarks = (await _uow.EntryMarkRepository.Get(
        //                                            filter: e => e.SubjectGroupId == uniSubjectGroupDataSet.Id && e.MajorDetailId == majorDetail.Id && (e.Year == Consts.YEAR_2019 || e.Year == Consts.YEAR_2020)))
        //                                            .Select(e => _mapper.Map<UniEntryMarkDataSet>(e)).OrderBy(e => e.Year).ToList();
        //            uniSubjectGroupDataSet.EntryMarks = entryMarks;
        //        }
        //        uniSubjectGroupDataSets = uniSubjectGroupDataSets.Where(s => s.EntryMarks.Any()).ToList();
        //        uniMajorDataSet.SubjectGroups = uniSubjectGroupDataSets;
        //    }
        //    universityDataSet.Majors = uniMajorDataSets;
        //    if (universityDataSet == null)
        //    {
        //        response.Succeeded = false;
        //        if (response.Errors == null)
        //        {
        //            response.Errors = new List<string>();
        //        }
        //        response.Errors.Add("Trường đại học này không tồn tại!");
        //    } else
        //    {
        //        response.Succeeded = true;
        //        response.Data = universityDataSet;
        //    }
            return response;  
        }

        public async Task<Response<AdminUniversityDataSet>> CreateNewAnUniversity(CreateUniversityDataset createUniversityDataset)
        {
            Response<AdminUniversityDataSet> response = new Response<AdminUniversityDataSet>();
        //    if (createUniversityDataset.Name.Equals("") || createUniversityDataset.Code.Equals("") || (createUniversityDataset.Status != 0 && createUniversityDataset.Status != Consts.STATUS_ACTIVE))
        //    {
        //        response.Succeeded = false;
        //        if (response.Errors == null)
        //        {
        //            response.Errors = new List<string>();
        //        }
        //        response.Errors.Add("Các thông tin cần thiết không hợp lệ!");
        //        return response;
        //    }
                
        //    Models.University ExistUni = await _uow.UniversityRepository.GetFirst(filter: u => u.Code.Equals(createUniversityDataset.Code));
        //    if (ExistUni != null)
        //    {
        //        response.Succeeded = false;
        //        if (response.Errors == null)
        //        {
        //            response.Errors = new List<string>();
        //        }
        //        response.Errors.Add("Trường này đã tồn tại!");
        //        return response;
        //    }
        //    Models.University university = _mapper.Map<Models.University>(createUniversityDataset);
        //    _uow.UniversityRepository.Insert(university);
        //    int result = await _uow.CommitAsync();
        //    if (result > 0)
        //    {
        //        response.Succeeded = true;
        //        response.Data = _mapper.Map<AdminUniversityDataSet>(university);
        //    } else
        //    {
        //        response.Succeeded = false;
        //        if (response.Errors == null)
        //        {
        //            response.Errors = new List<string>();
        //        }
        //        response.Errors.Add("Lỗi hệ thống!");
        //    }
            return response;
        }

        public async Task<Response<AdminUniversityDataSet>> UpdateUniversity(AdminUniversityDataSet adminUniversityDataSet)
        {
            Response<AdminUniversityDataSet> response = new Response<AdminUniversityDataSet>();
            if (adminUniversityDataSet.Name.Equals("") || adminUniversityDataSet.Code.Equals("") || (adminUniversityDataSet.Status != Consts.STATUS_ACTIVE && adminUniversityDataSet.Status != Consts.STATUS_INACTIVE))
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Các thông tin cần thiết không hợp lệ!");
                return response;
            }
            Models.University existUni = await _uow.UniversityRepository.GetFirst(filter: u => u.Code.Equals(adminUniversityDataSet.Code.Trim()));
            if (existUni != null && existUni.Id != adminUniversityDataSet.Id)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Mã trường đại học đã tồn tại!");
                return response;

            }
            Models.University updatedUni = await _uow.UniversityRepository.GetById(adminUniversityDataSet.Id);
            if (updatedUni == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Trường này không tồn tại!");
                return response;
            }
            //Upload logo to Firebase block

            IFormFile logoImage = adminUniversityDataSet.File;
            if (logoImage != null)
            {
                if (Consts.IMAGE_EXTENSIONS.Contains(Path.GetExtension(logoImage.FileName).ToUpperInvariant()))
                {

                    using (var ms = new MemoryStream())
                    {
                        logoImage.CopyTo(ms);
                        ms.Position = 0;
                        if (ms != null && ms.Length > 0)
                        {
                            var auth = new FirebaseAuthProvider(new FirebaseConfig(Consts.API_KEY));
                            var firebaseAuth = await auth.SignInWithEmailAndPasswordAsync(Consts.AUTH_MAIL, Consts.AUTH_PASSWORD);

                            // you can use CancellationTokenSource to cancel the upload midway
                            var cancellation = new CancellationTokenSource();

                            var task = new FirebaseStorage(
                                Consts.BUCKET,
                                new FirebaseStorageOptions
                                {
                                    ThrowOnCancel = true, // when you cancel the upload, exception is thrown. By default no exception is thrown
                                    AuthTokenAsyncFactory = () => Task.FromResult(firebaseAuth.FirebaseToken),
                                })
                                .Child(Consts.LOGO_FOLDER)
                                .Child(adminUniversityDataSet.Code + Path.GetExtension(logoImage.FileName))
                                .PutAsync(ms, cancellation.Token);

                            adminUniversityDataSet.LogoUrl = await task;
                        }

                    }
                }
            }


            updatedUni.Code = adminUniversityDataSet.Code;
            updatedUni.Name = adminUniversityDataSet.Name;
            updatedUni.Address = adminUniversityDataSet.Address;
            updatedUni.LogoUrl = adminUniversityDataSet.LogoUrl;
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
                response.Succeeded = true;
                response.Data = _mapper.Map<AdminUniversityDataSet>(updatedUni);
            } else
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống!");
            }
            return response;
        }

        public async Task<Response<bool>> AddMajorToUniversity(AddingMajorUniversityParam addingMajorUniversityParam)
        {
            Response<bool> response = new Response<bool>();
            //MajorDetail majorDetail = null;

            //MajorDetail existedMajorDetail = await _uow.MajorDetailRepository
            //           .GetFirst(m => m.MajorId == addingMajorUniversityParam.MajorId
            //           && m.UniversityId == addingMajorUniversityParam.UniversityId
            //           && m.TrainingProgramId == addingMajorUniversityParam.TrainingProgramId);
            //if (existedMajorDetail != null)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Ngành này đã tồn tại trong trường!");
            //    return response;
            //}
            //majorDetail = new MajorDetail()
            //{
            //    MajorId = addingMajorUniversityParam.MajorId,
            //    UniversityId = addingMajorUniversityParam.UniversityId,
            //    TrainingProgramId = addingMajorUniversityParam.TrainingProgramId,
            //    MajorCode = addingMajorUniversityParam.MajorCode,
            //};

            //_uow.MajorDetailRepository.Insert(majorDetail);

            //if ((await _uow.CommitAsync()) <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Lỗi hệ thống!");
            //    return response;
            //}

            //AdmissionCriterion admissionCriterion = new AdmissionCriterion()
            //{
            //    MajorDetailId = majorDetail.Id,
            //    Quantity = addingMajorUniversityParam.NumberOfStudents,
            //    Year = 2021
            //};

            //_uow.AdmissionCriterionRepository.Insert(admissionCriterion);

            //if ((await _uow.CommitAsync()) <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Lỗi hệ thống!");
            //    return response;
            //}

            //if (addingMajorUniversityParam.SubjectGroups != null && addingMajorUniversityParam.SubjectGroups.Any())
            //{
            //    foreach (UniSubjectGroupDataSet uniSubjectGroupDataSet in addingMajorUniversityParam.SubjectGroups)
            //    {
            //        if (uniSubjectGroupDataSet.EntryMarks == null || !uniSubjectGroupDataSet.EntryMarks.Any())
            //        {
            //            _uow.EntryMarkRepository.Insert(new EntryMark()
            //                {
            //                    MajorDetailId = majorDetail.Id,
            //                    Mark = null,
            //                    Year = Consts.YEAR_2020,
            //                    SubjectGroupId = uniSubjectGroupDataSet.Id
            //                }
            //            );
            //        }
            //        else
            //        {
            //            foreach (UniEntryMarkDataSet uniEntryMarkDataSet in uniSubjectGroupDataSet.EntryMarks)
            //            {
            //                _uow.EntryMarkRepository.Insert(new EntryMark()
            //                    {
            //                        MajorDetailId = majorDetail.Id,
            //                        Mark = uniEntryMarkDataSet.Mark,
            //                        Year = uniEntryMarkDataSet.Year,
            //                        SubjectGroupId = uniSubjectGroupDataSet.Id
            //                    }
            //                );
            //            }
            //        }
            //    }

            //    if ((await _uow.CommitAsync()) <= 0)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Lỗi hệ thống!");
            //        return response;
            //    } else
            //    {
            //        response.Succeeded = true;
            //        response.Data = true;
            //    }
            //    return response;
            //}
            //response.Succeeded = false;
            //if (response.Errors == null)
            //{
            //    response.Errors = new List<string>();
            //}
            //response.Errors.Add("Danh sách khối không được trống!");
            return response;
        }
        public async Task<Response<bool>> UpdateMajorOfUniversity(UpdatingMajorUniversityParam updatingMajorUniversityParam)
        {
            Response<bool> response = new Response<bool>();
            //if (updatingMajorUniversityParam.MajorId < 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Id ngành không hợp lệ!");
            //    return response;
            //}

            //bool isMajorExisted = (await _uow.MajorRepository.GetById(updatingMajorUniversityParam.MajorId)) != null;

            //if (!isMajorExisted)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Ngành này không tồn tại!");
            //    return response;
            //}

            //MajorDetail majorDetail = await _uow.MajorDetailRepository
            //    .GetFirst(filter: m => m.MajorId == updatingMajorUniversityParam.MajorId 
            //    && m.UniversityId == updatingMajorUniversityParam.UniversityId 
            //    && m.TrainingProgramId == updatingMajorUniversityParam.OldTrainingProgramId);
            //if (majorDetail == null)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Không có ngành này trong trường!");
            //    return response;
            //}

            //if (updatingMajorUniversityParam.OldTrainingProgramId != updatingMajorUniversityParam.NewTrainingProgramId)
            //{
            //    MajorDetail exitedUpdateMajorDetail = await _uow.MajorDetailRepository
            //    .GetFirst(filter: m => m.MajorId == updatingMajorUniversityParam.MajorId
            //    && m.UniversityId == updatingMajorUniversityParam.UniversityId
            //    && m.TrainingProgramId == updatingMajorUniversityParam.NewTrainingProgramId);

            //    if (exitedUpdateMajorDetail != null)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Ngành này đã tồn tại trong trường!");
            //        return response;
            //    }
            //}

            //majorDetail.TrainingProgramId = updatingMajorUniversityParam.NewTrainingProgramId;
            //majorDetail.MajorCode = updatingMajorUniversityParam.MajorCode;
            //_uow.MajorDetailRepository.Update(majorDetail);

            //AdmissionCriterion admissionCriterion = (await _uow.AdmissionCriterionRepository
            //    .Get(filter: a => a.MajorDetailId == majorDetail.Id && a.Year == 2021)).FirstOrDefault();

            //if (admissionCriterion != null)
            //{
            //    admissionCriterion.Quantity = updatingMajorUniversityParam.NumberOfStudents;
            //    _uow.AdmissionCriterionRepository.Update(admissionCriterion);
            //} else
            //{
            //    admissionCriterion = new AdmissionCriterion()
            //    {
            //        MajorDetailId = majorDetail.Id,
            //        Year = 2021,
            //        Quantity = updatingMajorUniversityParam.NumberOfStudents
            //    };
            //    _uow.AdmissionCriterionRepository.Insert(admissionCriterion);
            //}

            //if ((await _uow.CommitAsync()) <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Lỗi hệ thống!");
            //    return response;
            //}

            //foreach (UpdatingUniSubjectGroupDataSet updatingUniSubjectGroupDataSet in updatingMajorUniversityParam.SubjectGroups)
            //{
            //   if (updatingUniSubjectGroupDataSet.IsDeleted)
            //   {
            //        foreach (UniEntryMarkDataSet entryMark in updatingUniSubjectGroupDataSet.EntryMarks)
            //        {
            //            _uow.EntryMarkRepository.Delete(entryMark.Id);
            //        }
            //   } else
            //   {
            //        foreach (UniEntryMarkDataSet entryMark in updatingUniSubjectGroupDataSet.EntryMarks)
            //        {
            //            if (entryMark.Id < 0)
            //            {
            //                if (entryMark.Mark < 0)
            //                {
            //                    response.Succeeded = false;
            //                    if (response.Errors == null)
            //                    {
            //                        response.Errors = new List<string>();
            //                    }
            //                    response.Errors.Add("Điểm chuẩn phải lớn hơn 0!");
            //                    return response;
            //                }
            //                EntryMark existedEntryMark = await _uow.EntryMarkRepository
            //                    .GetFirst(filter: s => s.SubjectGroupId == updatingUniSubjectGroupDataSet.Id
            //                                    && s.MajorDetailId == majorDetail.Id && s.Year == entryMark.Year);
            //                if (existedEntryMark != null)
            //                {
            //                    return null;
            //                }
            //                EntryMark newEntryMark = new EntryMark()
            //                {
            //                    MajorDetailId = majorDetail.Id,
            //                    Mark = entryMark.Mark,
            //                    SubjectGroupId = updatingUniSubjectGroupDataSet.Id,
            //                    Year = entryMark.Year
            //                };
            //                _uow.EntryMarkRepository.Insert(newEntryMark);
            //            } else
            //            {
            //                EntryMark existedEntryMark = await _uow.EntryMarkRepository.GetById(entryMark.Id);
            //                existedEntryMark.Mark = entryMark.Mark;
            //                _uow.EntryMarkRepository.Update(existedEntryMark);
            //            }
            //        }
            //    }
               
            //}

            //if ((await _uow.CommitAsync()) <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Lỗi hệ thống!");
            //} else
            //{
            //    response.Succeeded = true;
            //    response.Data = true; ;
            //}
            
            return response;
        }
    }

}
