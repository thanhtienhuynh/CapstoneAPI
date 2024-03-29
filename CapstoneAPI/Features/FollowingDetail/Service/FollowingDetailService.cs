﻿using AutoMapper;
using CapstoneAPI.Features.FollowingDetail.DataSet;
using CapstoneAPI.Features.Major.DataSet;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Features.TrainingProgram.DataSet;
using CapstoneAPI.Features.University.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.FollowingDetail.Service
{
    public class FollowingDetailService : IFollowingDetailService
    {

        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<FollowingDetailService>();

        public FollowingDetailService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<Models.FollowingDetail>> AddFollowingDetail(AddFollowingDetailParam followingDetailParam, string token)
        {
            Response<Models.FollowingDetail> response = new Response<Models.FollowingDetail>();
            try
            {
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

                Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
                Models.Season previousSeason = await _uow.SeasonRepository.GetPreviousSeason();

                if (currentSeason == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mùa tuyển sinh chưa được kích hoạt!");
                    return response;
                }
                SeasonDataSet previousSeasonDataSet = new SeasonDataSet
                {
                    Id = previousSeason.Id,
                    Name = previousSeason.Name
                };

                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                    return response;
                }

                MajorDetail majorDetail = await _uow.MajorDetailRepository
                                            .GetFirst(filter: m => m.MajorId == followingDetailParam.MajorId
                                                    && m.TrainingProgramId == followingDetailParam.TrainingProgramId
                                                    && m.UniversityId == followingDetailParam.UniversityId
                                                    && m.SeasonId == currentSeason.Id && m.Status == Consts.STATUS_ACTIVE);

                if (majorDetail == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Trường này không tồn tại!");
                    return response;
                }


                IEnumerable<EntryMark> entryMarks = await _uow.EntryMarkRepository.Get(filter: e => e.SubAdmissionCriterion.AdmissionMethodId == AdmissionMethodTypes.THPTQG
                                                                && e.SubAdmissionCriterion.AdmissionCriterion.MajorDetailId == majorDetail.Id
                                                                && e.MajorSubjectGroup.MajorId == followingDetailParam.MajorId
                                                                && e.MajorSubjectGroup.SubjectGroupId == followingDetailParam.SubjectGroupId
                                                                && e.Status == Consts.STATUS_ACTIVE,
                                                                includeProperties: "SubAdmissionCriterion");

                EntryMark entryMark = null;


                if (entryMarks.Where(e => e.SubAdmissionCriterion.Gender == followingDetailParam.SubjectGroupParam.Gender).Any())
                {
                    entryMarks = entryMarks.Where(e => e.SubAdmissionCriterion.Gender == followingDetailParam.SubjectGroupParam.Gender);
                } else
                {
                    entryMarks = entryMarks.Where(e => e.SubAdmissionCriterion.Gender == null);
                }

                if (entryMarks.Where(e => e.SubAdmissionCriterion.ProvinceId == followingDetailParam.SubjectGroupParam.ProvinceId).Any())
                {
                    entryMark = entryMarks.FirstOrDefault(e => e.SubAdmissionCriterion.ProvinceId == followingDetailParam.SubjectGroupParam.ProvinceId);
                } else
                {
                    entryMark = entryMarks.FirstOrDefault(e => e.SubAdmissionCriterion.ProvinceId == null);
                }

                if (entryMark == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Khối điểm này không tồn tại!");
                    return response;
                }

                Models.FollowingDetail followingDetail = await _uow.FollowingDetailRepository
                                                .GetFirst(filter: u => u.UserId == user.Id
                                                            && u.EntryMarkId == entryMark.Id && u.Status == Consts.STATUS_ACTIVE);
                if (followingDetail == null)
                {
                    List<int> currentEntryMarkIds = (await _uow.EntryMarkRepository
                                    .Get(filter: e => e.SubAdmissionCriterionId == entryMark.SubAdmissionCriterionId))
                                    .Select(e => e.Id).ToList();
                    IEnumerable<Models.Rank> ranks = (await _uow.FollowingDetailRepository
                                                               .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId) && f.Status == Consts.STATUS_ACTIVE,
                                                                   includeProperties: "Rank"))
                                                               .Select(u => u.Rank).Where(r => r != null);
                    followingDetail = new Models.FollowingDetail()
                    {
                        EntryMarkId = entryMark.Id,
                        UserId = user.Id,
                        IsReceiveNotification = true,
                        Status = Consts.STATUS_ACTIVE,
                        Rank = new Models.Rank()
                        {
                            TranscriptTypeId = followingDetailParam.SubjectGroupParam.TranscriptTypeId,
                            IsUpdate = true,
                            TotalMark = followingDetailParam.TotalMark,
                            UpdatedDate = JWTUtils.GetCurrentTimeInVN(),
                            Position = followingDetailParam.Position
                        }
                    };
                    _uow.FollowingDetailRepository.Insert(followingDetail);
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Quan tâm không thành công, lỗi hệ thống!");
                        return response;
                    }
                }
                response.Succeeded = true;
                response.Data = followingDetail;
            } catch (Exception ex)
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

        public async Task<Response<bool>> RemoveFollowingDetail(int followingDetailId, string token)
        {
            Response<bool> response = new Response<bool>();
            Models.User user = await _uow.UserRepository.GetUserByToken(token);

            if (user == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            if (!user.IsActive)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                return response;
            }

            Models.FollowingDetail followingDetail = await _uow.FollowingDetailRepository
                .GetFirst(filter: f => f.Id == followingDetailId && f.Status == Consts.STATUS_ACTIVE
                    && f.UserId == user.Id,
                    includeProperties: "Rank");
            if (followingDetail == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa quan tâm trường này!");
                return response;  
            }

            using var tran = _uow.GetTransaction();
            try
            {
                followingDetail.Rank.IsUpdate = true;
                
                followingDetail.Status = Consts.STATUS_INACTIVE;
                _uow.FollowingDetailRepository.Update(followingDetail);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bỏ quan tâm không thành công, lỗi hệ thống!");
                    return response;
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add(ex.Message);
                return response;
            }

            response.Succeeded = true;
            response.Data = true;
            return response;
        }

        public async Task<Response<bool>> RemoveFollowingDetailInSubAdmission(int followingDetailId, string token)
        {
            Response<bool> response = new Response<bool>();
            Models.User user = await _uow.UserRepository.GetUserByToken(token);

            if (user == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            if (!user.IsActive)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Tài khoản của bạn đã bị khóa!");
                return response;
            }

            Models.FollowingDetail followingDetail = await _uow.FollowingDetailRepository.GetFirst(filter: 
                f => f.Id == followingDetailId && f.Status == Consts.STATUS_ACTIVE
                    && f.UserId == user.Id, includeProperties: "Rank,EntryMark");
            if (followingDetail == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa quan tâm trường này!");
                return response;
            }

            IEnumerable<Models.FollowingDetail> sameSubAdmissions = await _uow.FollowingDetailRepository.Get(
                f => f.EntryMark.SubAdmissionCriterionId == followingDetail.EntryMark.SubAdmissionCriterionId
                    && f.Status == Consts.STATUS_ACTIVE && f.UserId == user.Id,
                includeProperties: "Rank");
            using var tran = _uow.GetTransaction();
            try
            {
                foreach (var followDetail in sameSubAdmissions)
                {
                    followingDetail.Rank.IsUpdate = true;
                    followingDetail.Status = Consts.STATUS_INACTIVE;
                }
                
                _uow.FollowingDetailRepository.UpdateRange(sameSubAdmissions);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bỏ quan tâm không thành công, lỗi hệ thống!");
                    return response;
                }
                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                _log.Error(ex.ToString());
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add(ex.Message);
                return response;
            }

            response.Succeeded = true;
            response.Data = true;
            return response;
        }

        public async Task<Response<IEnumerable<FollowingDetailGroupByMajorDataSet>>> GetFollowingDetailGroupByMajorDataSets(string token)
        {
            Response<IEnumerable<FollowingDetailGroupByMajorDataSet>> response = new Response<IEnumerable<FollowingDetailGroupByMajorDataSet>>();

            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
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
                
                IEnumerable<Models.FollowingDetail> followingDetails = await _uow.FollowingDetailRepository.
                                    Get(filter: u => u.UserId == user.Id && u.EntryMark.Status == Consts.STATUS_ACTIVE && u.Status == Consts.STATUS_ACTIVE,
                                    includeProperties: "EntryMark,Rank,EntryMark.MajorSubjectGroup.SubjectGroup," +
                                    "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram," +
                                    "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University," +
                                    "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major");
                if (followingDetails == null || !followingDetails.Any())
                {
                    response.Succeeded = true;
                    return response;
                }
                IEnumerable<IGrouping<Models.Major, Models.FollowingDetail>> followingDetailGroups = followingDetails.GroupBy(u => u.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major);

                List<FollowingDetailGroupByMajorDataSet> result = new List<FollowingDetailGroupByMajorDataSet>();
                // cái này lấy major name
                foreach (IGrouping<Models.Major, Models.FollowingDetail> followingDetailInMajor in followingDetailGroups)
                {
                    FollowingDetailGroupByMajorDataSet followingDetailGroupByMajorDataSet = _mapper.Map<FollowingDetailGroupByMajorDataSet>(followingDetailInMajor.Key);
                    IEnumerable<IGrouping<Models.TrainingProgram, Models.FollowingDetail>> groupByTrainingProgram =
                                followingDetailInMajor.GroupBy(g => g.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram);
                    List<TrainingProgramGroupByMajorDataSet> trainingProgramGroupByMajorDataSets = new List<TrainingProgramGroupByMajorDataSet>();
                    foreach (IGrouping<Models.TrainingProgram, Models.FollowingDetail> trainingProgramGroup in groupByTrainingProgram)
                    {
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
                        TrainingProgramGroupByMajorDataSet trainingProgramGroupByMajorDataSet = _mapper.Map<TrainingProgramGroupByMajorDataSet>(trainingProgramGroup.Key);
                        List<UniversityGroupByTrainingProgramDataSet> universityGroupByTrainingProgramDataSets = new List<UniversityGroupByTrainingProgramDataSet>();
                        var sameSubAdmissionFollowingDetails = trainingProgramGroup
                            .OrderBy(f => f.Rank.Position).GroupBy(f => f.EntryMark.SubAdmissionCriterion);
                        foreach (var sameSubAdmissionFollowingDetail in sameSubAdmissionFollowingDetails)
                        {
                            List<OtherSubjectGroup> others = new List<OtherSubjectGroup>();
                            UniversityGroupByTrainingProgramDataSet universityGroupByTrainingProgramDataSet
                                = new UniversityGroupByTrainingProgramDataSet();
                            for (var i = 0; i < sameSubAdmissionFollowingDetail.Count(); i++)
                            {
                                Models.FollowingDetail temp = sameSubAdmissionFollowingDetail.AsEnumerable().ToList()[i];
                                if (i == 0)
                                {
                                    List<int> currentEntryMarkIds = (await _uow.EntryMarkRepository
                                        .Get(filter: e => e.Status == Consts.STATUS_ACTIVE
                                        && e.SubAdmissionCriterionId == temp.EntryMark.SubAdmissionCriterionId))
                                        .Select(e => e.Id).ToList();
                                    currentSeasonDataSet.EntryMark = temp.EntryMark.Mark;
                                    currentSeasonDataSet.NumberOfStudents = temp.EntryMark.SubAdmissionCriterion.Quantity;
                                    await SetUpPreviousSeasonDataSet(temp, previousSeason, previousSeasonDataSet);
                                    universityGroupByTrainingProgramDataSet = _mapper.Map<UniversityGroupByTrainingProgramDataSet>
                                        (temp.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University);
                                    universityGroupByTrainingProgramDataSet.FollowingDetailId = temp.Id;
                                    universityGroupByTrainingProgramDataSet.MajorCode = temp.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.MajorCode;
                                    universityGroupByTrainingProgramDataSet.PositionOfUser = temp.Rank.Position;
                                    universityGroupByTrainingProgramDataSet.TotalUserCared = (await _uow.FollowingDetailRepository
                                    .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId) && f.Status == Consts.STATUS_ACTIVE)).Count();
                                    universityGroupByTrainingProgramDataSet.SeasonDataSets = new List<SeasonDataSet>()
                                    {
                                     previousSeasonDataSet,
                                     currentSeasonDataSet
                                    };
                                    universityGroupByTrainingProgramDataSet.SubjectGroupId = temp.EntryMark.MajorSubjectGroup.SubjectGroupId;
                                    universityGroupByTrainingProgramDataSet.SubjectGroupCode = temp.EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode;
                                    universityGroupByTrainingProgramDataSet.RankingMark = temp.Rank.TotalMark;
                                    
                                } else
                                {
                                    others.Add(new OtherSubjectGroup
                                    {
                                        Id = temp.EntryMark.MajorSubjectGroup.SubjectGroupId,
                                        Name = temp.EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode,
                                        Mark = temp.Rank.TotalMark,
                                        RankTypeId = temp.Rank.TranscriptTypeId
                                    });
                                }
                            }
                            universityGroupByTrainingProgramDataSet.OtherSubjectGroups = others;
                            universityGroupByTrainingProgramDataSets.Add(universityGroupByTrainingProgramDataSet);
                        }

                        trainingProgramGroupByMajorDataSet.UniversityGroupByTrainingProgramDataSets = universityGroupByTrainingProgramDataSets;
                        trainingProgramGroupByMajorDataSets.Add(trainingProgramGroupByMajorDataSet);
                    }
                    followingDetailGroupByMajorDataSet.TrainingProgramGroupByMajorDataSets = trainingProgramGroupByMajorDataSets;
                    result.Add(followingDetailGroupByMajorDataSet);
                }
                if (result.Count <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa có ngành quan tâm");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = result;
                }
            } catch (Exception ex)
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

        public async Task<Response<IEnumerable<FollowingDetailGroupByUniversityDataSet>>> GetFollowingDetailGroupByUniversityDataSets(string token)
        {
            Response<IEnumerable<FollowingDetailGroupByUniversityDataSet>> response = new Response<IEnumerable<FollowingDetailGroupByUniversityDataSet>>();

            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn đã bị khóa!");
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


                IEnumerable<Models.FollowingDetail> followingDetails = await _uow.FollowingDetailRepository.
                                    Get(filter: u => u.UserId == user.Id && u.EntryMark.Status == Consts.STATUS_ACTIVE && u.Status == Consts.STATUS_ACTIVE,
                                    includeProperties: "EntryMark,Rank," +
                                    "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram," +
                                    "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University," +
                                    "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major," +
                                    "EntryMark.MajorSubjectGroup.SubjectGroup");
                if (followingDetails == null || !followingDetails.Any())
                {
                    response.Succeeded = true;
                    return response;
                }

                IEnumerable<IGrouping<Models.University, Models.FollowingDetail>> followingDetailGroups = followingDetails.GroupBy(u => u.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University);

                List<FollowingDetailGroupByUniversityDataSet> result = new List<FollowingDetailGroupByUniversityDataSet>();
                foreach (IGrouping<Models.University, Models.FollowingDetail> followingDetailInUni in followingDetailGroups)
                {
                    FollowingDetailGroupByUniversityDataSet followingDetailDataSet = _mapper.Map<FollowingDetailGroupByUniversityDataSet>(followingDetailInUni.Key);
                    List<TrainingProgramGroupByUniversityDataSet> trainingProgramGroupByUniversityDataSets = new List<TrainingProgramGroupByUniversityDataSet>();

                    IEnumerable<IGrouping<Models.TrainingProgram, Models.FollowingDetail>> groupByTrainingProgram = followingDetailInUni.GroupBy(m => m.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram);
                    foreach (IGrouping<Models.TrainingProgram, Models.FollowingDetail> followingDetailInTrainingProgram in groupByTrainingProgram)
                    {

                        TrainingProgramGroupByUniversityDataSet trainingProgramGroupByUniversityDataSet =
                                                    _mapper.Map<TrainingProgramGroupByUniversityDataSet>(followingDetailInTrainingProgram.Key);
                        List<MajorGroupByTrainingProgramDataSet> majorGroupByTrainingProgramDataSets 
                            = new List<MajorGroupByTrainingProgramDataSet>();
                        var sameSubAdmissionFollowingDetails = followingDetailInTrainingProgram
                           .OrderBy(f => f.Rank.Position).GroupBy(f => f.EntryMark.SubAdmissionCriterion);
                        List<OtherSubjectGroup> others = new List<OtherSubjectGroup>();
                        foreach (var sameSubAdmission in sameSubAdmissionFollowingDetails)
                        {
                            var majorGroupByTrainingProgramDataSet = new MajorGroupByTrainingProgramDataSet();
                            for (var i = 0; i < sameSubAdmission.Count(); i++)
                            {
                                Models.FollowingDetail followingDetail = sameSubAdmission.AsEnumerable().ToList()[i];
                                if (i == 0)
                                {
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
                                    List<int> currentEntryMarkIds = (await _uow.EntryMarkRepository
                                            .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == followingDetail.EntryMark.SubAdmissionCriterionId))
                                            .Select(e => e.Id).ToList();
                                    currentSeasonDataSet.EntryMark = followingDetail.EntryMark.Mark;
                                    currentSeasonDataSet.NumberOfStudents = followingDetail.EntryMark.SubAdmissionCriterion.Quantity;
                                    await SetUpPreviousSeasonDataSet(followingDetail, previousSeason, previousSeasonDataSet);
                                    majorGroupByTrainingProgramDataSet = new MajorGroupByTrainingProgramDataSet
                                    {
                                        FollowingDetailId = followingDetail.Id,
                                        Id = followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major.Id,
                                        Code = followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major.Code,
                                        Name = followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major.Name,
                                        MajorCode = followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.MajorCode,
                                        PositionOfUser = followingDetail.Rank.Position,
                                        RankingMark = followingDetail.Rank.TotalMark,
                                        RankTypeId = followingDetail.Rank.TranscriptTypeId,
                                        SeasonDataSets = new List<SeasonDataSet>()
                                        {
                                            previousSeasonDataSet,
                                            currentSeasonDataSet
                                        },
                                        TotalUserCared = (await _uow.FollowingDetailRepository.Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId) && f.Status == Consts.STATUS_ACTIVE)).Count(),
                                        SubjectGroupId = followingDetail.EntryMark.MajorSubjectGroup.SubjectGroupId,
                                        SubjectGroupCode = followingDetail.EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode,
                                    };
                                }
                                else
                                {
                                    others.Add(new OtherSubjectGroup
                                    {
                                        Id = followingDetail.EntryMark.MajorSubjectGroup.SubjectGroupId,
                                        Name = followingDetail.EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode,
                                        Mark = followingDetail.Rank.TotalMark,
                                        RankTypeId = followingDetail.Rank.TranscriptTypeId
                                    });
                                }
                            }
                            majorGroupByTrainingProgramDataSet.OtherSubjectGroups = others;
                            majorGroupByTrainingProgramDataSets.Add(majorGroupByTrainingProgramDataSet);
                        }
                        trainingProgramGroupByUniversityDataSet.MajorGroupByTrainingProgramDataSets = majorGroupByTrainingProgramDataSets;
                        trainingProgramGroupByUniversityDataSets.Add(trainingProgramGroupByUniversityDataSet);
                    }
                    followingDetailDataSet.TrainingProgramGroupByUniversityDataSets = trainingProgramGroupByUniversityDataSets;
                    result.Add(followingDetailDataSet);
                }
                if (result.Count <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa có ngành quan tâm");
                }
                else
                {
                    response.Succeeded = true;
                    response.Data = result;
                }
            } catch (Exception ex)
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

        public async Task<Response<UserFollowingDetail>> GetFollowingDetailById(int id, string token)
        {
            Response<UserFollowingDetail> response = new Response<UserFollowingDetail>();
            UserFollowingDetail userFollowingDetail = new UserFollowingDetail();

            try
            {
                Models.User user = await _uow.UserRepository.GetUserByToken(token);

                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa đăng nhập!");
                    return response;
                }

                Models.FollowingDetail followingDetail = await _uow.FollowingDetailRepository.GetFirst(filter: f => f.Id == id
                                                                        && f.EntryMark.Status == Consts.STATUS_ACTIVE && f.Status == Consts.STATUS_ACTIVE
                                                                        && f.UserId == user.Id,
                                                                        includeProperties: "EntryMark,Rank.TranscriptType,EntryMark.MajorSubjectGroup.SubjectGroup," +
                                                                       "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram," +
                                                                        "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University," +
                                                                        "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major");
                if (followingDetail == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Thông tin theo dõi không tồn tại!");
                    return response;
                }

                RankingInformation rankingInformation = new RankingInformation();

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

                await SetUpPreviousSeasonDataSet(followingDetail, previousSeason, previousSeasonDataSet);

                IEnumerable<Models.FollowingDetail> followingDetails = await _uow.FollowingDetailRepository
                    .Get(filter: u => u.EntryMark.Status == Consts.STATUS_ACTIVE
                                    && u.Status == Consts.STATUS_ACTIVE
                                    && u.EntryMark.SubAdmissionCriterionId == followingDetail.EntryMark.SubAdmissionCriterionId,
                    includeProperties: "User,EntryMark,Rank.TranscriptType,EntryMark.MajorSubjectGroup.SubjectGroup," +
                                        "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram," +
                                        "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University," +
                                        "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major",
                    orderBy: u => u.OrderBy(u => u.Rank.Position));

                IEnumerable<IGrouping<Models.User, Models.FollowingDetail>> followingDetailsGroupsByUser =
                    followingDetails.GroupBy(u => u.User);
                List<OtherSubjectGroup> others = new List<OtherSubjectGroup>();
                foreach (var gr in followingDetailsGroupsByUser)
                {
                    if (gr.Key.Id == user.Id)
                    {
                        List<Models.FollowingDetail> temp = gr.AsEnumerable().ToList();
                        for(var i = 0; i < temp.Count; i++)
                        {
                            if ( i > 0)
                            {
                                others.Add(new OtherSubjectGroup()
                                {
                                    Id = temp[i].EntryMark.MajorSubjectGroup.SubjectGroupId,
                                    Name = temp[i].EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode,
                                    Mark = temp[i].Rank.TotalMark,
                                    RankTypeId = temp[i].Rank.TranscriptTypeId
                                });
                            }
                        }
                    }
                }
                followingDetails = followingDetailsGroupsByUser.Select(g => g.FirstOrDefault());

                IEnumerable<IGrouping<TranscriptType, Models.FollowingDetail>> followingDetailsGroupsByTranscriptType =
                        followingDetails.Where(f => f.Rank.TotalMark >= previousSeasonDataSet.EntryMark).GroupBy(u => u.Rank.TranscriptType)
                        .OrderByDescending(g => g.Key.Priority);

                var rankingUserInformationGroupByTranscriptTypes = new List<RankingUserInformationGroupByTranscriptType>();

                foreach (IGrouping<TranscriptType, Models.FollowingDetail> followingDetailsGroup in followingDetailsGroupsByTranscriptType)
                {
                    List<RankingUserInformation> rankingUserInformations = new List<RankingUserInformation>();
                    RankingUserInformationGroupByTranscriptType rankingUserInformationGroupByTranscriptType = new RankingUserInformationGroupByTranscriptType();
                    foreach (Models.FollowingDetail followingDetailGr in followingDetailsGroup)
                    {
                        RankingUserInformation rankingUserInformation = _mapper.Map<RankingUserInformation>(followingDetailGr.User);
                        rankingUserInformation.GroupCode = followingDetailGr.EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode;
                        rankingUserInformation.Position = followingDetailGr.Rank.Position;
                        rankingUserInformation.TotalMark = followingDetailGr.Rank.TotalMark;
                        rankingUserInformations.Add(rankingUserInformation);
                    }
                    rankingUserInformationGroupByTranscriptType.Id = followingDetailsGroup.Key.Id;
                    rankingUserInformationGroupByTranscriptType.Name = followingDetailsGroup.Key.Name;
                    rankingUserInformationGroupByTranscriptType.RankingUserInformations = rankingUserInformations.OrderBy(r => r.Position).ThenByDescending(r => r.TotalMark).ToList();
                    rankingUserInformationGroupByTranscriptTypes.Add(rankingUserInformationGroupByTranscriptType);
                }

                //Check out of uni
                var outRankingUserInformationGroupByTranscriptType = new RankingUserInformationGroupByTranscriptType();
                List<RankingUserInformation> outRankingUserInformations = new List<RankingUserInformation>();
                foreach (var outFollowing in followingDetails.Where(f => f.Rank.TotalMark < previousSeasonDataSet.EntryMark))
                {
                    RankingUserInformation rankingUserInformation = _mapper.Map<RankingUserInformation>(outFollowing.User);
                    rankingUserInformation.GroupCode = outFollowing.EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode;
                    rankingUserInformation.Position = outFollowing.Rank.Position;
                    rankingUserInformation.TotalMark = outFollowing.Rank.TotalMark;
                    outRankingUserInformations.Add(rankingUserInformation);
                }
                outRankingUserInformationGroupByTranscriptType.Name = "Không đủ điều kiện";
                outRankingUserInformationGroupByTranscriptType.RankingUserInformations = outRankingUserInformations.OrderBy(r => r.Position).ThenByDescending(r => r.TotalMark).ToList();
                rankingUserInformationGroupByTranscriptTypes.Add(outRankingUserInformationGroupByTranscriptType);
                //En check out of uni

                rankingInformation.PositionOfUser = followingDetail.Rank.Position;
                currentSeasonDataSet.EntryMark = followingDetail.EntryMark.Mark;
                currentSeasonDataSet.NumberOfStudents = followingDetail.EntryMark.SubAdmissionCriterion.Quantity;
                rankingInformation.SeasonDataSets = new List<SeasonDataSet>
                {
                    previousSeasonDataSet,
                    currentSeasonDataSet
                };
                rankingInformation.SubjectGroupId = followingDetail.EntryMark.MajorSubjectGroup.SubjectGroupId;
                rankingInformation.SubjectGroupCode = followingDetail.EntryMark.MajorSubjectGroup.SubjectGroup.GroupCode;
                rankingInformation.RankingMark = followingDetail.Rank.TotalMark;
                rankingInformation.TotalUserCared = rankingUserInformationGroupByTranscriptTypes.Count();
                rankingInformation.OtherSubjectGroups = others;
                rankingInformation.RankTypeId = followingDetail.Rank.TranscriptTypeId;
                userFollowingDetail.RankingInformation = rankingInformation;
                userFollowingDetail.RankingUserInformationsGroupByTranscriptType = rankingUserInformationGroupByTranscriptTypes;
                userFollowingDetail.UniversityDataSet = _mapper.Map<DetailUniversityDataSet>(followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University);
                userFollowingDetail.MajorDataSet = _mapper.Map<AdminMajorDataSet>(followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major);
                userFollowingDetail.TrainingProgramDataSet = _mapper.Map<AdminTrainingProgramDataSet>(followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram);

                response.Succeeded = true;
                response.Data = userFollowingDetail;
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

        private async Task SetUpPreviousSeasonDataSet(Models.FollowingDetail followingDetail, Models.Season previousSeason, SeasonDataSet previousSeasonDataSet)
        {
            MajorDetail previousMajorDetail = await _uow.MajorDetailRepository.GetFirst(filter: m => m.SeasonId == previousSeason.Id
                                        && m.MajorId == followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.MajorId
                                        && m.UniversityId == followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.UniversityId
                                        && m.TrainingProgramId == followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgramId
                                        && m.Status == Consts.STATUS_ACTIVE,
                                        includeProperties: "AdmissionCriterion.SubAdmissionCriteria");

            if (previousMajorDetail != null && previousMajorDetail.AdmissionCriterion != null && previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria != null
               && previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.Status == Consts.STATUS_ACTIVE).Any())
            {
                IEnumerable<SubAdmissionCriterion> previousSubAdmissionCriterias = previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                .Where(a => a.AdmissionMethodId == AdmissionMethodTypes.THPTQG && a.Status == Consts.STATUS_ACTIVE);

                //Check ptts cho giới tính riêng

                IEnumerable<SubAdmissionCriterion> subPreviousSubAdmissionCriteriasByGender = previousSubAdmissionCriterias.Where(s => s.Gender == followingDetail.EntryMark.SubAdmissionCriterion.Gender);
                if (subPreviousSubAdmissionCriteriasByGender.Any())
                {
                    previousSubAdmissionCriterias = subPreviousSubAdmissionCriteriasByGender;
                }
                else
                {
                    previousSubAdmissionCriterias = previousSubAdmissionCriterias.Where(s => s.Gender == null);

                }

                SubAdmissionCriterion subPreviousSubAdmissionCriteria = previousSubAdmissionCriterias.Where(s => s.ProvinceId == followingDetail.EntryMark.SubAdmissionCriterion.ProvinceId).FirstOrDefault();
                if (subPreviousSubAdmissionCriteria == null)
                {
                    subPreviousSubAdmissionCriteria = previousSubAdmissionCriterias.Where(s => s.ProvinceId == null).FirstOrDefault();
                }

                if (subPreviousSubAdmissionCriteria != null)
                {
                    EntryMark previousEntryMark = (await _uow.EntryMarkRepository
                        .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == subPreviousSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
                            includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup,SubAdmissionCriterion"))
                            .Where(e => e.MajorSubjectGroupId == followingDetail.EntryMark.MajorSubjectGroupId).FirstOrDefault();
                    if (previousEntryMark != null)
                    {
                        previousSeasonDataSet.EntryMark = previousEntryMark.Mark;
                        previousSeasonDataSet.NumberOfStudents = previousEntryMark.SubAdmissionCriterion.Quantity;
                    }
                }
            }
        }
    }
}
