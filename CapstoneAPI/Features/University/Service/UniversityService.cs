﻿using AutoMapper;
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
using CapstoneAPI.Filters;
using CapstoneAPI.Filters.University;
using System.Linq.Expressions;
using CapstoneAPI.Filters.MajorDetail;
using Serilog;
using CapstoneAPI.Features.University.DataSet;
using CapstoneAPI.Features.FollowingDetail.DataSet;
using CapstoneAPI.Features.SubjectGroup.DataSet;

namespace CapstoneAPI.Features.University.Service
{
    public class UniversityService : IUniversityService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<UniversityService>();

        public UniversityService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<TrainingProgramBasedUniversityDataSet>>> GetUniversityBySubjectGroupAndMajor(UniversityParam universityParam, string token)
        {
            Response<IEnumerable<TrainingProgramBasedUniversityDataSet>> response = new Response<IEnumerable<TrainingProgramBasedUniversityDataSet>>();
            try
            {
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

                Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
                Models.Season previousSeason = await _uow.SeasonRepository.GetPreviousSeason();

                if (universityParam.TotalMark <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Điểm của bạn không đủ điều kiện xét tuyển đại học!");
                    return response;
                }

                //Lấy ra tất cả các trường va hệ có ngành đã chọn
                List<MajorDetail> majorDetails = (await _uow.MajorDetailRepository
                    .Get(filter: w => w.Status == Consts.STATUS_ACTIVE && w.MajorId == universityParam.MajorId,
                        includeProperties: "University,TrainingProgram,AdmissionCriterion,AdmissionCriterion.SubAdmissionCriteria"))
                    .ToList();
                if (!majorDetails.Any())
                {
                    response.Succeeded = true;
                    response.Data = trainingProgramBasedUniversityDataSets;
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
                            || !currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.Status == Consts.STATUS_ACTIVE).Any()
                            || previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria == null
                            || !previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.Status == Consts.STATUS_ACTIVE).Any())
                        {
                            continue;
                        }

                        List<SubAdmissionCriterion> currentSubAdmissionCriterias = currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                            .Where(a => a.Status == Consts.STATUS_ACTIVE && a.AdmissionMethodId == 1 && (a.Gender == universityParam.Gender || a.Gender == null)
                             && (a.ProvinceId == universityParam.ProvinceId || a.ProvinceId == null)).ToList();
                        List<SubAdmissionCriterion> previousSubAdmissionCriterias = previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                            .Where(a => a.Status == Consts.STATUS_ACTIVE && a.AdmissionMethodId == 1 && (a.Gender == universityParam.Gender || a.Gender == null)
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
                                .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == currentSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
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
                                .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == previousSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
                                    includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup,SubAdmissionCriterion"))
                                    .Where(e => e.MajorSubjectGroup.SubjectGroupId == universityParam.SubjectGroupId
                                                && e.MajorSubjectGroup.MajorId == universityParam.MajorId).FirstOrDefault();
                            if (previousEntryMark != null)
                            {
                                break;
                            }
                        }

                        if (currentEntryMark == null || previousEntryMark == null || previousEntryMark.Mark == null || previousEntryMark.Mark > universityParam.TotalMark)
                        {
                            continue;
                        }

                        previousSeasonDataSet.EntryMark = previousEntryMark.Mark;
                        previousSeasonDataSet.NumberOfStudents = previousEntryMark.SubAdmissionCriterion.Quantity;
                        currentSeasonDataSet.NumberOfStudents = currentEntryMark.SubAdmissionCriterion.Quantity;
                        List<int> currentEntryMarkIds = (await _uow.EntryMarkRepository
                                                        .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == currentEntryMark.SubAdmissionCriterionId))
                                                        .Select(e => e.Id).ToList();
                        seasonDataSets.Add(previousSeasonDataSet);
                        seasonDataSets.Add(currentSeasonDataSet);
                        trainingProgramDataSet.SeasonDataSets = seasonDataSets;
                        trainingProgramDataSet.NumberOfCaring = (await _uow.FollowingDetailRepository
                            .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId) && f.Status == Consts.STATUS_ACTIVE)).Count();
                        if (userId > 0)
                        {
                            trainingProgramDataSet.FollowingDetail = _mapper.Map<FollowingDetailDataSet>(await _uow.FollowingDetailRepository
                                                                                        .GetFirst(filter: f => f.UserId == userId
                                                                                        && f.Status == Consts.STATUS_ACTIVE
                                                                                        && f.EntryMarkId == currentEntryMark.Id));
                        }
                        IEnumerable<Models.Rank> ranks = (await _uow.FollowingDetailRepository
                                                                .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId) && f.Status == Consts.STATUS_ACTIVE,
                                                                    includeProperties: "Rank"))
                                                                .Select(u => u.Rank).Where(r => r != null);
                        trainingProgramDataSet.Rank = _uow.RankRepository.CalculateRank(universityParam.TranscriptTypeId, universityParam.TotalMark, ranks);

                        trainingProgramDataSets.Add(trainingProgramDataSet);
                    }
                    if (trainingProgramDataSets.Any())
                    {
                        trainingProgramBasedUniversityDataSet.TrainingProgramSets = trainingProgramDataSets;
                        trainingProgramBasedUniversityDataSets.Add(trainingProgramBasedUniversityDataSet);
                    }
                }
                response.Succeeded = true;
                response.Data = trainingProgramBasedUniversityDataSets;
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

        public async Task<Response<MockTestBasedUniversity>> CalculaterUniversityByMockTestMarks(MockTestsUniversityParam universityParam, string token)
        {
            Response<MockTestBasedUniversity> response = new Response<MockTestBasedUniversity>();
            try
            {
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
                MockTestBasedUniversity mockTestBasedUniversity = new MockTestBasedUniversity();

                Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
                Models.Season previousSeason = await _uow.SeasonRepository.GetPreviousSeason();

                List<SubjectGroupDetail> subjectGroupDetails = (await _uow.SubjecGroupDetailRepository.Get(s => s.SubjectGroupId == universityParam.SubjectGroupId)).ToList();
                double totalMark = await CalculateSubjectGroupMark(universityParam.Marks, subjectGroupDetails);

                if (totalMark == 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Điểm thi thử của bạn không đủ điều kiện xét tuyển đại học!");
                    return response;
                }

                //Lấy ra tất cả các trường va hệ có ngành đã chọn
                List<MajorDetail> majorDetails = (await _uow.MajorDetailRepository
                    .Get(filter: w => w.Status == Consts.STATUS_ACTIVE && w.MajorId == universityParam.MajorId,
                        includeProperties: "University,TrainingProgram,AdmissionCriterion,AdmissionCriterion.SubAdmissionCriteria"))
                    .ToList();
                if (majorDetails == null || !majorDetails.Any())
                {
                    response.Succeeded = true;
                    response.Data = mockTestBasedUniversity;
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
                            || !currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.Status == Consts.STATUS_ACTIVE).Any()
                            || previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria == null
                            || !previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria.Where(s => s.Status == Consts.STATUS_ACTIVE).Any())
                        {
                            continue;
                        }

                        List<SubAdmissionCriterion> currentSubAdmissionCriterias = currentMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                            .Where(a => a.Status == Consts.STATUS_ACTIVE && a.AdmissionMethodId == 1 && (a.Gender == universityParam.Gender || a.Gender == null)
                             && (a.ProvinceId == universityParam.ProvinceId || a.ProvinceId == null)).ToList();
                        List<SubAdmissionCriterion> previousSubAdmissionCriterias = previousMajorDetail.AdmissionCriterion.SubAdmissionCriteria
                            .Where(a => a.Status == Consts.STATUS_ACTIVE && a.AdmissionMethodId == 1 && (a.Gender == universityParam.Gender || a.Gender == null)
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
                                .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == currentSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
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
                                .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == previousSubAdmissionCriteria.Id && e.MajorSubjectGroupId != null,
                                    includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup,SubAdmissionCriterion"))
                                    .Where(e => e.MajorSubjectGroup.SubjectGroupId == universityParam.SubjectGroupId
                                                && e.MajorSubjectGroup.MajorId == universityParam.MajorId).FirstOrDefault();
                            if (previousEntryMark != null)
                            {
                                break;
                            }
                        }

                        if (currentEntryMark == null || previousEntryMark == null || previousEntryMark.Mark == null || previousEntryMark.Mark > totalMark)
                        {
                            continue;
                        }

                        previousSeasonDataSet.EntryMark = previousEntryMark.Mark;
                        previousSeasonDataSet.NumberOfStudents = previousEntryMark.SubAdmissionCriterion.Quantity;
                        currentSeasonDataSet.NumberOfStudents = currentEntryMark.SubAdmissionCriterion.Quantity;
                        List<int> currentEntryMarkIds = (await _uow.EntryMarkRepository
                                                        .Get(filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == currentEntryMark.SubAdmissionCriterionId))
                                                        .Select(e => e.Id).ToList();
                        seasonDataSets.Add(previousSeasonDataSet);
                        seasonDataSets.Add(currentSeasonDataSet);
                        trainingProgramDataSet.SeasonDataSets = seasonDataSets;
                        trainingProgramDataSet.NumberOfCaring = (await _uow.FollowingDetailRepository
                            .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId) && f.Status == Consts.STATUS_ACTIVE)).Count();
                        if (userId > 0)
                        {
                            trainingProgramDataSet.FollowingDetail = _mapper.Map<FollowingDetailDataSet>(await _uow.FollowingDetailRepository.GetFirst(filter: f => f.UserId == userId
                                                                                        && f.Status == Consts.STATUS_ACTIVE
                                                                                        && f.EntryMarkId == currentEntryMark.Id));
                        }
                        IEnumerable<Models.Rank> ranks = (await _uow.FollowingDetailRepository
                                                                .Get(filter: f => currentEntryMarkIds.Contains(f.EntryMarkId) && f.Status == Consts.STATUS_ACTIVE,
                                                                    includeProperties: "Rank"))
                                                                .Select(u => u.Rank).Where(r => r != null);
                        trainingProgramDataSet.Rank = _uow.RankRepository.CalculateRank(universityParam.TranscriptTypeId, totalMark, ranks);

                        trainingProgramDataSets.Add(trainingProgramDataSet);
                    }
                    if (trainingProgramDataSets.Any())
                    {
                        trainingProgramBasedUniversityDataSet.TrainingProgramSets = trainingProgramDataSets;
                        trainingProgramBasedUniversityDataSets.Add(trainingProgramBasedUniversityDataSet);
                    }
                }

                mockTestBasedUniversity.SubjectGroupId = universityParam.SubjectGroupId;
                mockTestBasedUniversity.MajorId = universityParam.MajorId;
                mockTestBasedUniversity.TotalMark = totalMark;
                mockTestBasedUniversity.TrainingProgramBasedUniversityDataSets = trainingProgramBasedUniversityDataSets;
                response.Succeeded = true;
                response.Data = mockTestBasedUniversity;
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
            return response; ;
        }

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

        public async Task<Response<IEnumerable<AdminUniversityDataSet>>> GetAllUniversities()
        {
            Response<IEnumerable<AdminUniversityDataSet>> response = new Response<IEnumerable<AdminUniversityDataSet>>();
            try
            {
                IEnumerable<AdminUniversityDataSet> adminUniversityDataSets = (await _uow.UniversityRepository.Get(filter: u => u.Status == Consts.STATUS_ACTIVE)).
                Select(s => _mapper.Map<AdminUniversityDataSet>(s));


                response.Data = adminUniversityDataSets;
                response.Succeeded = true;
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

        public async Task<PagedResponse<List<AdminUniversityDataSet>>> GetAllUniversitiesForStudents(PaginationFilter validFilter,
            UniversityFilterForStudent universityFilter)
        {
            PagedResponse<List<AdminUniversityDataSet>> result = new PagedResponse<List<AdminUniversityDataSet>>();
            try
            {
                Expression<Func<Models.University, bool>> filter = null;

                filter = a => (string.IsNullOrEmpty(universityFilter.Name) || a.Name.Contains(universityFilter.Name))
                                && a.Status == Consts.STATUS_ACTIVE;

                Func<IQueryable<Models.University>, IOrderedQueryable<Models.University>> order = null;
                switch (universityFilter.Order)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.Code);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.Code);
                        break;
                    case 2:
                        order = order => order.OrderByDescending(a => a.Name);
                        break;
                    case 3:
                        order = order => order.OrderBy(a => a.Name);
                        break;
                    case 4:
                        order = order => order.OrderByDescending(a => a.TuitionType);
                        break;
                    case 5:
                        order = order => order.OrderBy(a => a.TuitionType);
                        break;
                }


                IEnumerable<Models.University> universities = await _uow.UniversityRepository
                    .Get(filter: filter, orderBy: order,
                    first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);


                var adminUniversityDataSet = universities.Select(m => _mapper.Map<AdminUniversityDataSet>(m)).ToList();
                var totalRecords = _uow.UniversityRepository.Count(filter);
                result = PaginationHelper.CreatePagedReponse(adminUniversityDataSet, validFilter, totalRecords);
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return result;
        }
        public async Task<PagedResponse<List<AdminUniversityDataSet>>> GetUniversities(PaginationFilter validFilter,
            UniversityFilter universityFilter)
        {
            PagedResponse<List<AdminUniversityDataSet>> result = new PagedResponse<List<AdminUniversityDataSet>>();
            try
            {
                Expression<Func<Models.University, bool>> filter = null;

                filter = a => (string.IsNullOrEmpty(universityFilter.Name) || a.Name.Contains(universityFilter.Name))
                && (string.IsNullOrEmpty(universityFilter.Code) || a.Code.Contains(universityFilter.Code))
                && (universityFilter.TuitionType == null || universityFilter.TuitionType == a.TuitionType)
                && (universityFilter.Status == null || a.Status == universityFilter.Status);

                Func<IQueryable<Models.University>, IOrderedQueryable<Models.University>> order = null;
                switch (universityFilter.Order)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.Code);
                        break;
                    case 1:
                        order = order => order.OrderBy(a => a.Code);
                        break;
                    case 2:
                        order = order => order.OrderByDescending(a => a.Name);
                        break;
                    case 3:
                        order = order => order.OrderBy(a => a.Name);
                        break;
                    case 4:
                        order = order => order.OrderByDescending(a => a.TuitionType);
                        break;
                    case 5:
                        order = order => order.OrderBy(a => a.TuitionType);
                        break;
                }


                IEnumerable<Models.University> universities = await _uow.UniversityRepository
                    .Get(filter: filter, orderBy: order,
                    first: validFilter.PageSize, offset: (validFilter.PageNumber - 1) * validFilter.PageSize);


                var adminUniversityDataSet = universities.Select(m => _mapper.Map<AdminUniversityDataSet>(m)).ToList();
                var totalRecords = _uow.UniversityRepository.Count(filter);
                result = PaginationHelper.CreatePagedReponse(adminUniversityDataSet, validFilter, totalRecords);
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return result;
        }
        public async Task<Response<List<UniMajorNonPagingDataSet>>> GetMajorDetailInUniversityNonPaging(MajorDetailParam majorDetailParam)
        {
            List<UniMajorNonPagingDataSet> uniMajorDataSets = new List<UniMajorNonPagingDataSet>();
            Response<List<UniMajorNonPagingDataSet>> result = new Response<List<UniMajorNonPagingDataSet>>();
            try
            {
                IEnumerable<MajorDetail> majorDetails = await _uow.MajorDetailRepository
                    .Get(filter: m => m.UniversityId == majorDetailParam.UniversityId && m.SeasonId == majorDetailParam.SeasonId && m.Status == Consts.STATUS_ACTIVE,
                    includeProperties: "Major,Season,AdmissionCriterion,TrainingProgram");

                IEnumerable<IGrouping<Models.Major, MajorDetail>> groupbyMajor = majorDetails.GroupBy(m => m.Major);
                foreach (IGrouping<Models.Major, MajorDetail> item in groupbyMajor)
                {
                    UniMajorNonPagingDataSet uniMajorDataSet = new UniMajorNonPagingDataSet();
                    uniMajorDataSet.UniversityId = majorDetailParam.UniversityId;
                    uniMajorDataSet.MajorId = item.Key.Id;
                    uniMajorDataSet.MajorName = item.Key.Name;
                    uniMajorDataSet.MajorCode = item.Key.Code;
                    foreach (MajorDetail detailWithAMajor in item)
                    {
                        MajorDetailUniNonPagingDataSet majorDetailUniDataSet = new MajorDetailUniNonPagingDataSet
                        {
                            Id = detailWithAMajor.Id,
                            TrainingProgramId = detailWithAMajor.TrainingProgram.Id,
                            TrainingProgramName = detailWithAMajor.TrainingProgram.Name,
                            MajorDetailCode = detailWithAMajor.MajorCode,
                            AdmissionQuantity = detailWithAMajor.AdmissionCriterion.Quantity,
                            SeasonId = detailWithAMajor.Season.Id,
                            SeasonName = detailWithAMajor.Season.Name
                        };

                        if (uniMajorDataSet.MajorDetailUnies == null)
                        {
                            uniMajorDataSet.MajorDetailUnies = new List<MajorDetailUniNonPagingDataSet>();
                        }
                        uniMajorDataSet.MajorDetailUnies.Add(majorDetailUniDataSet);
                    }
                    uniMajorDataSets.Add(uniMajorDataSet);
                }
                result.Data = uniMajorDataSets;
                result.Succeeded = true;
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return result;
        }
        public async Task<PagedResponse<List<UniMajorDataSet>>> GetMajorDetailInUniversity(PaginationFilter validFilter, MajorDetailFilter majorDetailFilter)
        {
            PagedResponse<List<UniMajorDataSet>> result = new PagedResponse<List<UniMajorDataSet>>();
            try
            {
                List<UniMajorDataSet> uniMajorDataSets = new List<UniMajorDataSet>();
                Expression<Func<MajorDetail, bool>> filter = null;

                filter = a => (string.IsNullOrEmpty(majorDetailFilter.MajorName) || a.Major.Name.Contains(majorDetailFilter.MajorName))
                && (string.IsNullOrEmpty(majorDetailFilter.MajorCode) || a.Major.Code.Contains(majorDetailFilter.MajorCode))
                && (a.Status == Consts.STATUS_ACTIVE)
                && (majorDetailFilter.UniversityId == a.UniversityId)
                && (majorDetailFilter.SeasonId == a.SeasonId);


                Func<IQueryable<MajorDetail>, IOrderedQueryable<MajorDetail>> order = null;
                switch (majorDetailFilter.Order)
                {
                    case 0:
                        order = order => order.OrderByDescending(a => a.UpdatedDate);
                        break;
                    case 1:
                        order = order => order.OrderByDescending(a => a.Major.Code);
                        break;
                    case 2:
                        order = order => order.OrderBy(a => a.Major.Code);
                        break;
                    case 3:
                        order = order => order.OrderByDescending(a => a.Major.Name);
                        break;
                    case 4:
                        order = order => order.OrderBy(a => a.Major.Name);
                        break;
                }

                IEnumerable<Models.MajorDetail> majorDetails = await _uow.MajorDetailRepository
                .Get(filter: filter, orderBy: order, includeProperties: "Major,Season,AdmissionCriterion,TrainingProgram");

                IEnumerable<IGrouping<Models.Major, Models.MajorDetail>> groupbyMajor = majorDetails.GroupBy(m => m.Major);
                foreach (IGrouping<Models.Major, Models.MajorDetail> item in groupbyMajor)
                {
                    UniMajorDataSet uniMajorDataSet = new UniMajorDataSet();
                    uniMajorDataSet.UniversityId = majorDetailFilter.UniversityId;
                    uniMajorDataSet.MajorId = item.Key.Id;
                    uniMajorDataSet.MajorName = item.Key.Name;
                    uniMajorDataSet.MajorCode = item.Key.Code;
                    foreach (Models.MajorDetail detailWithAMajor in item)
                    {
                        MajorDetailUniDataSet majorDetailUniDataSet = new MajorDetailUniDataSet();
                        majorDetailUniDataSet.Id = detailWithAMajor.Id;
                        majorDetailUniDataSet.TrainingProgramId = detailWithAMajor.TrainingProgram.Id;
                        majorDetailUniDataSet.TrainingProgramName = detailWithAMajor.TrainingProgram.Name;
                        majorDetailUniDataSet.MajorDetailCode = detailWithAMajor.MajorCode;
                        majorDetailUniDataSet.AdmissionQuantity = detailWithAMajor.AdmissionCriterion.Quantity;
                        majorDetailUniDataSet.SeasonId = detailWithAMajor.Season.Id;
                        majorDetailUniDataSet.SeasonName = detailWithAMajor.Season.Name;
                        IEnumerable<Models.SubAdmissionCriterion> subAdmissionCriteria = await _uow.SubAdmissionCriterionRepository.
                                                   Get(filter: s => s.AdmissionCriterionId == detailWithAMajor.AdmissionCriterion.MajorDetailId
                                                   && s.Status == Consts.STATUS_ACTIVE,
                                                   includeProperties: "Province,AdmissionMethod");
                        foreach (Models.SubAdmissionCriterion subAdmission in subAdmissionCriteria)
                        {
                            MajorDetailSubAdmissionDataSet majorDetailSubAdmissionDataSet = new MajorDetailSubAdmissionDataSet();
                            majorDetailSubAdmissionDataSet.Id = subAdmission.Id;
                            majorDetailSubAdmissionDataSet.Quantity = subAdmission.Quantity;
                            majorDetailSubAdmissionDataSet.ProvinceId = subAdmission.ProvinceId;
                            if (subAdmission.ProvinceId != null)
                            {
                                majorDetailSubAdmissionDataSet.ProvinceName = subAdmission.Province.Name;
                            }
                            majorDetailSubAdmissionDataSet.GenderId = subAdmission.Gender;
                            if (subAdmission.AdmissionMethod != null)
                            {
                                majorDetailSubAdmissionDataSet.AdmissionMethodId = subAdmission.AdmissionMethod.Id;
                                majorDetailSubAdmissionDataSet.AdmissionMethodName = subAdmission.AdmissionMethod.Name;
                            }
                            IEnumerable<Models.EntryMark> entryMarks = await _uow.EntryMarkRepository.Get(e => e.SubAdmissionCriterionId == subAdmission.Id
                            && e.Status == Consts.STATUS_ACTIVE,
                               includeProperties: "MajorSubjectGroup,MajorSubjectGroup.SubjectGroup");
                            foreach (Models.EntryMark entry in entryMarks)
                            {
                                MajorDetailEntryMarkDataset majorDetailEntryMarkDataset = new MajorDetailEntryMarkDataset();
                                majorDetailEntryMarkDataset.Id = entry.Id;
                                majorDetailEntryMarkDataset.Mark = entry.Mark;
                                majorDetailEntryMarkDataset.MajorSubjectGoupId = entry.MajorSubjectGroup.Id;
                                majorDetailEntryMarkDataset.SubjectGroupId = entry.MajorSubjectGroup.SubjectGroup.Id;
                                majorDetailEntryMarkDataset.SubjectGroupCode = entry.MajorSubjectGroup.SubjectGroup.GroupCode;
                                if (majorDetailSubAdmissionDataSet.MajorDetailEntryMarks == null)
                                {
                                    majorDetailSubAdmissionDataSet.MajorDetailEntryMarks = new List<MajorDetailEntryMarkDataset>();
                                }
                                majorDetailSubAdmissionDataSet.MajorDetailEntryMarks.Add(majorDetailEntryMarkDataset);
                            }
                            if (majorDetailUniDataSet.MajorDetailSubAdmissions == null)
                            {
                                majorDetailUniDataSet.MajorDetailSubAdmissions = new List<MajorDetailSubAdmissionDataSet>();
                            }
                            majorDetailUniDataSet.MajorDetailSubAdmissions.Add(majorDetailSubAdmissionDataSet);
                        }
                        if (uniMajorDataSet.MajorDetailUnies == null)
                        {
                            uniMajorDataSet.MajorDetailUnies = new List<MajorDetailUniDataSet>();
                        }
                        uniMajorDataSet.MajorDetailUnies.Add(majorDetailUniDataSet);
                    }
                    uniMajorDataSets.Add(uniMajorDataSet);
                }
                var totalRecords = uniMajorDataSets.Count;
                List<UniMajorDataSet> responseData = uniMajorDataSets.Skip((validFilter.PageNumber - 1) * validFilter.PageSize).Take(validFilter.PageSize).ToList();
                result = PaginationHelper.CreatePagedReponse(responseData, validFilter, totalRecords);
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                result.Succeeded = false;
                if (result.Errors == null)
                {
                    result.Errors = new List<string>();
                }
                result.Errors.Add("Lỗi hệ thống: " + ex.Message);
            }
            return result;
        }

        public async Task<Response<DetailUniversityDataSet>> GetDetailUniversity(int universityId)
        {
            Response<DetailUniversityDataSet> response = new Response<DetailUniversityDataSet>();
            try
            {
                Models.University university = await _uow.UniversityRepository.GetFirst(filter: u => u.Id == universityId);
                if (university == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Trường đại học này không tồn tại!");
                    return response;
                }
                DetailUniversityDataSet universityDataSet = _mapper.Map<DetailUniversityDataSet>(university);
                {
                    response.Succeeded = true;
                    response.Data = universityDataSet;
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

            try
            {
                Models.University ExistUni = await _uow.UniversityRepository.GetFirst(filter: u => u.Code.Equals(createUniversityDataset.Code));
                if (ExistUni != null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Trường này đã tồn tại!");
                    return response;
                }
                //Upload logo to Firebase block

                IFormFile logoImage = createUniversityDataset.File;
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
                                    .Child(createUniversityDataset.Code + Path.GetExtension(logoImage.FileName))
                                    .PutAsync(ms, cancellation.Token);
                                try
                                {
                                    createUniversityDataset.LogoUrl = await task;
                                }
                                catch
                                {
                                    createUniversityDataset.LogoUrl = null;
                                }
                            }

                        }
                    }
                }

                Models.University university = _mapper.Map<Models.University>(createUniversityDataset);
                _uow.UniversityRepository.Insert(university);
                int result = await _uow.CommitAsync();
                if (result > 0)
                {
                    response.Succeeded = true;
                    response.Data = _mapper.Map<AdminUniversityDataSet>(university);
                }
                else
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lỗi hệ thống!");
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

        public async Task<Response<AdminUniversityDataSet>> UpdateUniversity(AdminUniversityDataSet adminUniversityDataSet)
        {
            Response<AdminUniversityDataSet> response = new Response<AdminUniversityDataSet>();
            try
            {
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
                                try
                                {
                                    adminUniversityDataSet.LogoUrl = await task;
                                }
                                catch(Exception ex)
                                {
                                    _log.Error(ex.ToString());

                                }
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
                }
                else
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lỗi hệ thống!");
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

        public async Task<Response<bool>> AddMajorToUniversity(AddingMajorUniversityParam addingMajorUniversityParam)
        {
            Response<bool> response = new Response<bool>();
            MajorDetail majorDetail = null;

            //CHECK NULL
            MajorDetail existedMajorDetail = await _uow.MajorDetailRepository
                       .GetFirst(m => m.MajorId == addingMajorUniversityParam.MajorId
                       && m.UniversityId == addingMajorUniversityParam.UniversityId
                       && m.SeasonId == addingMajorUniversityParam.SeasonId
                       && m.TrainingProgramId == addingMajorUniversityParam.TrainingProgramId
                       && m.Status == Consts.STATUS_ACTIVE);
            if (existedMajorDetail != null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Ngành này đã tồn tại trong trường!");
                return response;
            }
            if (addingMajorUniversityParam.SubAdmissions == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Bạn chưa nhập chỉ tiêu phụ.");
                return response;
            }

            foreach (UniSubAdmissionDataSet uniSubAdmissionDataSet in addingMajorUniversityParam.SubAdmissions)
            {
                if (uniSubAdmissionDataSet.SubjectGroups == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Bạn chưa nhập tổ hợp môn ứng với ngành học");
                    return response;
                }
            }

            //INSERT
            using var tran = _uow.GetTransaction();
            try
            {
                majorDetail = new MajorDetail()
                {
                    MajorId = addingMajorUniversityParam.MajorId,
                    UniversityId = addingMajorUniversityParam.UniversityId,
                    TrainingProgramId = addingMajorUniversityParam.TrainingProgramId,
                    SeasonId = addingMajorUniversityParam.SeasonId,
                    MajorCode = addingMajorUniversityParam.MajorCode,
                    UpdatedDate = DateTime.UtcNow,
                    Status = Consts.STATUS_ACTIVE,
                };

                _uow.MajorDetailRepository.Insert(majorDetail);

                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Thêm ngành bị lỗi!");
                    return response;
                }

                AdmissionCriterion admissionCriterion = new AdmissionCriterion()
                {
                    MajorDetailId = majorDetail.Id,
                    Quantity = addingMajorUniversityParam.TotalAdmissionQuantity,
                };

                _uow.AdmissionCriterionRepository.Insert(admissionCriterion);

                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Thêm chỉ tiêu bị lỗi!");
                    return response;
                }

                var subAdmissionHasSameCondition = addingMajorUniversityParam.SubAdmissions.GroupBy(a => new
                {
                    a.GenderId,
                    a.ProvinceId,
                    a.AdmissionMethodId
                });
                foreach (var subDetail in subAdmissionHasSameCondition)
                {
                    List<int> listCheckMajorSubjectGroup = new List<int>();
                    foreach (var subAdmission in subDetail)
                    {
                        foreach (var subjectGroup in subAdmission.SubjectGroups)
                        {
                            listCheckMajorSubjectGroup.Add(subjectGroup.MajorSubjectGroupId);
                        }
                    }
                    if (listCheckMajorSubjectGroup.Count() != listCheckMajorSubjectGroup.Distinct().Count())
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Cùng loại chỉ tiêu không thể có các khối trùng nhau");
                        return response;
                    }
                }
                foreach (UniSubAdmissionDataSet uniSubAdmission in addingMajorUniversityParam.SubAdmissions)
                {
                    Models.SubAdmissionCriterion subAdmissionCriterion = new SubAdmissionCriterion
                    {
                        AdmissionCriterionId = admissionCriterion.MajorDetailId,
                        Quantity = uniSubAdmission.Quantity,
                        Gender = uniSubAdmission.GenderId,
                        ProvinceId = uniSubAdmission.ProvinceId,
                        AdmissionMethodId = uniSubAdmission.AdmissionMethodId,
                        Status = Consts.STATUS_ACTIVE,
                    };
                    _uow.SubAdmissionCriterionRepository.Insert(subAdmissionCriterion);

                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Thêm chỉ tiêu phụ bị lỗi!");
                        return response;
                    }
                    foreach (UniSubjectGroupDataSet item in uniSubAdmission.SubjectGroups)
                    {
                        Models.MajorSubjectGroup majorSubjectGroup = await _uow.MajorSubjectGroupRepository
                                            .GetFirst(m => m.Id == item.MajorSubjectGroupId && m.Status == Consts.STATUS_ACTIVE
                                                    && m.MajorId == addingMajorUniversityParam.MajorId);
                        if (majorSubjectGroup == null)
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Ngành này chưa có khối mà bạn thêm vào!");
                            return response;
                        }
                        Models.EntryMark entryMark = new EntryMark
                        {
                            MajorSubjectGroupId = majorSubjectGroup.Id,
                            SubAdmissionCriterionId = subAdmissionCriterion.Id,
                            Mark = item.EntryMarkPerGroup,
                            Status = Consts.STATUS_ACTIVE,
                        };
                        _uow.EntryMarkRepository.Insert(entryMark);
                    }
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Thêm điểm chuẩn bị lỗi!");
                        return response;
                    }
                }

                tran.Commit();
                response.Succeeded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                tran.Rollback();
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống!" + ex.Message);

            }

            return response;
        }

        public async Task<Response<bool>> UpdateMajorOfUniversity(UpdatingMajorUniversityParam updatingMajorUniversityParam)
        {
            Response<bool> response = new Response<bool>();
            if (updatingMajorUniversityParam.MajorDetailId <= 0
                || updatingMajorUniversityParam.Status < 0)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Thông tin không hợp lệ!");
                return response;
            }

            MajorDetail MajorDetailExisted = await _uow.MajorDetailRepository.GetFirst(filter: m => m.Id == updatingMajorUniversityParam.MajorDetailId
            && m.Status == Consts.STATUS_ACTIVE,
                includeProperties: "AdmissionCriterion");

            if (MajorDetailExisted == null)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Ngành này không tồn tại!");
                return response;
            }
            //Update Block
            using var tran = _uow.GetTransaction();
            try
            {
                MajorDetailExisted.MajorCode = updatingMajorUniversityParam.MajorCode;
                MajorDetailExisted.UpdatedDate = DateTime.Now;
                MajorDetailExisted.Status = updatingMajorUniversityParam.Status;
                _uow.MajorDetailRepository.Update(MajorDetailExisted);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Cập nhật ngành bị lỗi!");
                    return response;
                }

                //REMOVE MAJORDETAIL
                if (updatingMajorUniversityParam.Status == Consts.STATUS_INACTIVE)
                {
                    IEnumerable<Models.SubAdmissionCriterion> subAdmissionCriterias = await _uow.SubAdmissionCriterionRepository.Get(filter:
                       s => s.AdmissionCriterionId == MajorDetailExisted.Id && s.Status == Consts.STATUS_ACTIVE);
                    if (subAdmissionCriterias != null && subAdmissionCriterias.Count() > 0)
                    {
                        foreach (SubAdmissionCriterion aSubAdmission in subAdmissionCriterias)
                        {
                            IEnumerable<Models.EntryMark> entryMarks = await _uow.EntryMarkRepository.Get(filter: e =>
                            e.SubAdmissionCriterionId == aSubAdmission.Id && e.Status == Consts.STATUS_ACTIVE);
                            if (entryMarks != null && entryMarks.Count() >0)
                            {
                                foreach (EntryMark mark in entryMarks)
                                {
                                    mark.Status = Consts.STATUS_INACTIVE;
                                    _uow.EntryMarkRepository.Update(mark);
                                }
                            }
                            aSubAdmission.Status = Consts.STATUS_INACTIVE;
                            _uow.SubAdmissionCriterionRepository.Update(aSubAdmission);
                        }
                        if ((await _uow.CommitAsync()) <= 0)
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Lỗi hệ thống!");
                            return response;
                        }
                    }
                    tran.Commit();
                    response.Succeeded = true;
                }

                else
                {
                    //UPDATE ADMISSION QUANTITY
                    MajorDetailExisted.AdmissionCriterion.Quantity = updatingMajorUniversityParam.TotalAdmissionQuantity;

                    _uow.AdmissionCriterionRepository.Update(MajorDetailExisted.AdmissionCriterion);


                    //SUB ASSMISSION CAN NOT NULL
                    if (updatingMajorUniversityParam.UpdatingUniSubAdmissionParams == null
                        || updatingMajorUniversityParam.UpdatingUniSubAdmissionParams.Count < 1)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Một ngành học phải có chỉ tiêu!");
                        return response;
                    }

                    foreach (UpdatingUniSubAdmissionParam subAdmissionParam in updatingMajorUniversityParam.UpdatingUniSubAdmissionParams)
                    {
                        Models.SubAdmissionCriterion newSubAdmissionCriterion = new SubAdmissionCriterion();
                        newSubAdmissionCriterion.AdmissionCriterionId = MajorDetailExisted.Id;
                        newSubAdmissionCriterion.AdmissionMethodId = subAdmissionParam.AdmissionMethodId;
                        newSubAdmissionCriterion.Gender = subAdmissionParam.GenderId;
                        newSubAdmissionCriterion.ProvinceId = subAdmissionParam.ProvinceId;
                        newSubAdmissionCriterion.Quantity = subAdmissionParam.Quantity;
                        newSubAdmissionCriterion.Status = subAdmissionParam.Status;
                        if (subAdmissionParam.SubAdmissionId == null || subAdmissionParam.SubAdmissionId <= 0)
                        {
                            _uow.SubAdmissionCriterionRepository.Insert(newSubAdmissionCriterion);
                        }
                        else
                        {
                            newSubAdmissionCriterion.Id = (int)subAdmissionParam.SubAdmissionId;
                            _uow.SubAdmissionCriterionRepository.Update(newSubAdmissionCriterion);
                        }

                        if ((await _uow.CommitAsync()) <= 0)
                        {
                            response.Succeeded = false;
                            if (response.Errors == null)
                            {
                                response.Errors = new List<string>();
                            }
                            response.Errors.Add("Cập nhật chỉ tiêu phụ bị lỗi!");
                            return response;
                        }
                        if(subAdmissionParam.MajorDetailEntryMarkParams != null)
                        {
                            foreach (var entryMarkParam in subAdmissionParam.MajorDetailEntryMarkParams)
                            {
                                if (entryMarkParam.Mark < 0 || entryMarkParam.MajorSubjectGroupId <= 0)
                                {
                                    response.Succeeded = false;
                                    if (response.Errors == null)
                                    {
                                        response.Errors = new List<string>();
                                    }
                                    response.Errors.Add("Điểm chuẩn không hợp lệ");
                                    return response;
                                }

                                Models.MajorSubjectGroup majorSubjectGroup = await _uow.MajorSubjectGroupRepository.GetById(entryMarkParam.MajorSubjectGroupId);
                                if (majorSubjectGroup == null || majorSubjectGroup.Status != Consts.STATUS_ACTIVE)
                                {
                                    response.Succeeded = false;
                                    if (response.Errors == null)
                                    {
                                        response.Errors = new List<string>();
                                    }
                                    response.Errors.Add("Khối không hợp lệ");
                                    return response;
                                }


                                Models.EntryMark entryMark = new EntryMark
                                {
                                    SubAdmissionCriterionId = newSubAdmissionCriterion.Id,
                                    MajorSubjectGroupId = entryMarkParam.MajorSubjectGroupId,
                                    Mark = entryMarkParam.Mark,
                                    Status = entryMarkParam.Status
                                };
                                if (entryMarkParam.EntryMarkId == null || entryMarkParam.EntryMarkId <= 0)
                                {
                                    _uow.EntryMarkRepository.Insert(entryMark);

                                }
                                else
                                {
                                    entryMark.Id = (int)entryMarkParam.EntryMarkId;
                                    _uow.EntryMarkRepository.Update(entryMark);
                                }
                                if ((await _uow.CommitAsync()) <= 0)
                                {
                                    response.Succeeded = false;
                                    if (response.Errors == null)
                                    {
                                        response.Errors = new List<string>();
                                    }
                                    response.Errors.Add("Cập nhật điểm chuẩn bị lỗi!");
                                    return response;
                                }
                            }
                        }
                    }

                    tran.Commit();
                    response.Succeeded = true;
                }                
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                tran.Rollback();
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Lỗi hệ thống! " + ex.Message);
            }
            return response;
        }


    }

}