using AutoMapper;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.DataSets.University;
using CapstoneAPI.DataSets.UserMajorDetail;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.UserMajorDetail;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.FollowingDetail
{
    public class FollowingDetailService : IFollowingDetailService
    {

        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public FollowingDetailService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<Models.FollowingDetail>> AddUserMajorDetail(AddUserMajorDetailParam userMajorDetailParam, string token)
        {
            Response<Models.FollowingDetail> response = new Response<Models.FollowingDetail>();
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

            Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();

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

            if (userMajorDetailParam.SubjectGroupParam != null)
            {
                foreach (MarkParam markParam in userMajorDetailParam.SubjectGroupParam.Marks)
                {
                    int transcriptTypeId = userMajorDetailParam.SubjectGroupParam.TranscriptTypeId;
                    Transcript transcript = await _uow.TranscriptRepository
                                            .GetFirst(filter: t => t.SubjectId == transcriptTypeId
                                                        && t.UserId == userId
                                                        && t.TranscriptTypeId == userMajorDetailParam.SubjectGroupParam.TranscriptTypeId);
                    if (transcript == null)
                    {
                        _uow.TranscriptRepository.Insert(new Transcript()
                        {
                            Mark = markParam.Mark,
                            SubjectId = markParam.SubjectId,
                            UserId = userId,
                            TranscriptTypeId = userMajorDetailParam.SubjectGroupParam.TranscriptTypeId,
                            DateRecord = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        transcript.Mark = markParam.Mark;
                        transcript.DateRecord = DateTime.UtcNow;
                        _uow.TranscriptRepository.Update(transcript);
                    }
                }
            }

            MajorDetail majorDetail = await _uow.MajorDetailRepository
                                        .GetFirst(filter: m => m.MajorId == userMajorDetailParam.MajorId
                                                && m.TrainingProgramId == userMajorDetailParam.TrainingProgramId
                                                && m.UniversityId == userMajorDetailParam.UniversityId
                                                && m.SeasonId == currentSeason.Id);

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

            //Models.FollowingDetail userMajorDetail = await _uow.FollowingDetailRepository
            //                                            .GetFirst(filter: u => u.UserId == userId
            //                                                        && u.MajorDetailId == majorDetail.Id);
            //if (userMajorDetail == null)
            //{
            //    IEnumerable<Models.Rank> ranks = (await _uow.UserMajorDetailRepository
            //                                                .Get(filter: u => u.MajorDetailId == majorDetail.Id, includeProperties: "Rank"))
            //                                                .Select(u => u.Rank).Where(r => r != null);
            //    userMajorDetail = new Models.UserMajorDetail()
            //    {
            //        MajorDetailId = majorDetail.Id,
            //        UserId = userId,
            //        MajorSubjectGroupId = userMajorDetailParam.SubjectGroupId,
            //        Status = Consts.STATUS_ACTIVE,
            //        Rank = new Models.Rank()
            //        {
            //            IsNew = true,
            //            IsReceiveNotification = true,
            //            RankTypeId = userMajorDetailParam.SubjectGroupParam.TranscriptTypeId,
            //            TotalMark = userMajorDetailParam.TotalMark,
            //            UpdatedDate = DateTime.UtcNow,
            //            Position = _uow.RankRepository.CalculateRank(userMajorDetailParam.SubjectGroupParam.TranscriptTypeId,
            //                                                        userMajorDetailParam.TotalMark, ranks)
            //        }
            //    };
            //    _uow.UserMajorDetailRepository.Insert(userMajorDetail);
            //    if ((await _uow.CommitAsync()) <= 0)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Quan tâm không thành công, lỗi hệ thống!");
            //        return response;
            //    }
            //}
            //response.Succeeded = true;
            //response.Data = userMajorDetail;
            return response;
        }

        public async Task<Response<Object>> RemoveUserMajorDetail(UpdateUserMajorDetailParam userMajorDetailParam, string token)
        {
            Response<Object> response = new Response<Object>();
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

            MajorDetail majorDetail = await _uow.MajorDetailRepository
                                        .GetFirst(filter: m => m.MajorId == userMajorDetailParam.MajorId
                                                && m.TrainingProgramId == userMajorDetailParam.TrainingProgramId
                                                && m.UniversityId == userMajorDetailParam.UniversityId);

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

            //Models.FollowingDetail userMajorDetail = await _uow.FollowingDetailRepository
            //                                            .GetFirst(filter: u => u.UserId == userId
            //                                                        && u.MajorDetailId == majorDetail.Id);
            //if (userMajorDetail != null)
            //{
            //    Models.Rank rank = await _uow.RankRepository.GetById(userMajorDetail.Id);
            //    if (rank != null)
            //    {
            //        _uow.RankRepository.Delete(userMajorDetail.Id);
            //        if ((await _uow.CommitAsync()) <= 0)
            //        {
            //            response.Succeeded = false;
            //            if (response.Errors == null)
            //            {
            //                response.Errors = new List<string>();
            //            }
            //            response.Errors.Add("Bỏ quan tâm không thành công, lỗi hệ thống!");
            //            return response;
            //        }
            //    }

            //    _uow.FollowingDetailRepository.Delete(userMajorDetail.Id);
            //    if ((await _uow.CommitAsync()) <= 0)
            //    {
            //        response.Succeeded = false;
            //        if (response.Errors == null)
            //        {
            //            response.Errors = new List<string>();
            //        }
            //        response.Errors.Add("Bỏ quan tâm không thành công, lỗi hệ thống!");
            //        return response;
            //    }
            //}

            response.Succeeded = true;

            return response;
        }

        public async Task<Response<IEnumerable<UserMajorDetailGroupByMajorDataSet>>> GetUserMajorDetailGroupByMajorDataSets(string token)
        {
            Response<IEnumerable<UserMajorDetailGroupByMajorDataSet>> response = new Response<IEnumerable<UserMajorDetailGroupByMajorDataSet>>();

            //if (token == null || token.Trim().Length == 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Bạn chưa đăng nhập!");
            //    return response;
            //}

            //string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            //if (userIdString == null || userIdString.Length <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tài khoản của bạn không tồn tại!");
            //    return response;
            //}

            //int userId = Int32.Parse(userIdString);
            //IEnumerable<Models.UserMajorDetail> userMajorDetails = await _uow.UserMajorDetailRepository.
            //                    Get(filter: u => u.UserId == userId,
            //                    includeProperties: "MajorDetail,Rank,MajorDetail.Major," +
            //                    "MajorDetail.University,SubjectGroup,MajorDetail.AdmissionCriteria,MajorDetail.EntryMarks,MajorDetail.TrainingProgram");
            //if (userMajorDetails == null || !userMajorDetails.Any())
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tài khoản của bạn không tồn tại!");
            //    return response;
            //}
            //IEnumerable<IGrouping<Models.Major, Models.UserMajorDetail>> userMajorDetailGroups = userMajorDetails.GroupBy(u => u.MajorDetail.Major);

            //List<UserMajorDetailGroupByMajorDataSet> result = new List<UserMajorDetailGroupByMajorDataSet>();
            //// cái này lấy major name
            //foreach (IGrouping<Models.Major, Models.UserMajorDetail> userMajorDetailInMajor in userMajorDetailGroups)
            //{
            //    UserMajorDetailGroupByMajorDataSet userMajorDetailGroupByMajorDataSet = _mapper.Map<UserMajorDetailGroupByMajorDataSet>(userMajorDetailInMajor.Key);
            //    IEnumerable<IGrouping<Models.TrainingProgram, Models.UserMajorDetail>> groupByTrainingProgram =
            //                userMajorDetailInMajor.GroupBy(g => g.MajorDetail.TrainingProgram);
            //    List<TrainingProgramGroupByMajorDataSet> trainingProgramGroupByMajorDataSets = new List<TrainingProgramGroupByMajorDataSet>();
            //    foreach (IGrouping<Models.TrainingProgram, Models.UserMajorDetail> trainingProgramGroup in groupByTrainingProgram)
            //    {
            //        TrainingProgramGroupByMajorDataSet trainingProgramGroupByMajorDataSet = _mapper.Map<TrainingProgramGroupByMajorDataSet>(trainingProgramGroup.Key);
            //        List<UniversityGroupByTrainingProgramDataSet> universityGroupByTrainingProgramDataSets = new List<UniversityGroupByTrainingProgramDataSet>();
            //        foreach (Models.UserMajorDetail userMajorDetail in trainingProgramGroup)
            //        {
            //            UniversityGroupByTrainingProgramDataSet universityGroupByTrainingProgramDataSet = _mapper.Map<UniversityGroupByTrainingProgramDataSet>(userMajorDetail.MajorDetail.University);

            //            universityGroupByTrainingProgramDataSet.MajorCode = userMajorDetail.MajorDetail.MajorCode;
            //            universityGroupByTrainingProgramDataSet.PositionOfUser = userMajorDetail.Rank?.Position;
            //            universityGroupByTrainingProgramDataSet.TotalUserCared = (await _uow.UserMajorDetailRepository.Get(filter: c => c.MajorDetailId == userMajorDetail.MajorDetailId)).Count();
            //            universityGroupByTrainingProgramDataSet.NumberOfStudent = userMajorDetail.MajorDetail.AdmissionCriteria
            //                                                                            .FirstOrDefault(u => u.Year == Consts.NEAREST_YEAR)?.Quantity;
            //            universityGroupByTrainingProgramDataSet.NewestEntryMark = userMajorDetail.MajorDetail.EntryMarks
            //                                                                        .FirstOrDefault(n => n.Year == Consts.NEAREST_YEAR && n.SubjectGroupId == userMajorDetail.SubjectGroupId)?.Mark;
            //            universityGroupByTrainingProgramDataSet.YearOfEntryMark = Consts.NEAREST_YEAR;
            //            universityGroupByTrainingProgramDataSet.SubjectGroupId = userMajorDetail.SubjectGroupId;
            //            universityGroupByTrainingProgramDataSet.SubjectGroupCode = userMajorDetail.SubjectGroup.GroupCode;
            //            universityGroupByTrainingProgramDataSet.RankingMark = userMajorDetail.Rank?.TotalMark;
            //            universityGroupByTrainingProgramDataSets.Add(universityGroupByTrainingProgramDataSet);
            //        }
            //        trainingProgramGroupByMajorDataSet.UniversityGroupByTrainingProgramDataSets = universityGroupByTrainingProgramDataSets;
            //        trainingProgramGroupByMajorDataSets.Add(trainingProgramGroupByMajorDataSet);
            //    }
            //    userMajorDetailGroupByMajorDataSet.TrainingProgramGroupByMajorDataSets = trainingProgramGroupByMajorDataSets;
            //    result.Add(userMajorDetailGroupByMajorDataSet);
            //}
            //if (result.Count <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Bạn chưa có ngành quan tâm");
            //}
            //else
            //{
            //    response.Succeeded = true;
            //    response.Data = result;
            //}
            return response;
        }

        public async Task<Response<IEnumerable<UserMajorDetailGroupByUniversityDataSet>>> GetUserMajorDetailGroupByUniversityDataSets(string token)
        {
            Response<IEnumerable<UserMajorDetailGroupByUniversityDataSet>> response = new Response<IEnumerable<UserMajorDetailGroupByUniversityDataSet>>();

            //if (token == null || token.Trim().Length == 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Bạn chưa đăng nhập!");
            //    return response;
            //}

            //string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            //if (userIdString == null || userIdString.Length <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tài khoản của bạn không tồn tại!");
            //    return response;
            //}

            //int userId = Int32.Parse(userIdString);

            //IEnumerable<Models.UserMajorDetail> userMajorDetails = await _uow.UserMajorDetailRepository.
            //                    Get(filter: u => u.UserId == userId,
            //                    includeProperties: "MajorDetail,Rank,MajorDetail.Major,MajorDetail.University,SubjectGroup," +
            //                    "MajorDetail.AdmissionCriteria,MajorDetail.EntryMarks,MajorDetail.TrainingProgram");

            //if (userMajorDetails == null || !userMajorDetails.Any())
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Tài khoản của bạn không tồn tại!");
            //    return response;
            //}

            //IEnumerable<IGrouping<Models.University, Models.UserMajorDetail>> userMajorDetailGroups = userMajorDetails.GroupBy(u => u.MajorDetail.University);

            //List<UserMajorDetailGroupByUniversityDataSet> result = new List<UserMajorDetailGroupByUniversityDataSet>();
            //foreach (IGrouping<Models.University, Models.UserMajorDetail> userMajorDetailInUni in userMajorDetailGroups)
            //{
            //    UserMajorDetailGroupByUniversityDataSet userMajorDetailDataSet = _mapper.Map<UserMajorDetailGroupByUniversityDataSet>(userMajorDetailInUni.Key);
            //    List<TrainingProgramGroupByUniversityDataSet> trainingProgramGroupByUniversityDataSets = new List<TrainingProgramGroupByUniversityDataSet>();

            //    IEnumerable<IGrouping<Models.TrainingProgram, Models.UserMajorDetail>> groupByTrainingProgram = userMajorDetailInUni.GroupBy(m => m.MajorDetail.TrainingProgram);
            //    foreach (IGrouping<Models.TrainingProgram, Models.UserMajorDetail> userMajorDetailInTrainingProgram in groupByTrainingProgram)
            //    {
            //        TrainingProgramGroupByUniversityDataSet trainingProgramGroupByUniversityDataSet = 
            //                                    _mapper.Map<TrainingProgramGroupByUniversityDataSet>(userMajorDetailInTrainingProgram.Key);
            //        List<MajorGroupByTrainingProgramDataSet> majorGroupByTrainingProgramDataSets = new List<MajorGroupByTrainingProgramDataSet>();
            //        foreach (Models.UserMajorDetail userMajorDetail in userMajorDetailInTrainingProgram)
            //        {
            //            MajorGroupByTrainingProgramDataSet majorGroupByTrainingProgramDataSet = new MajorGroupByTrainingProgramDataSet
            //            {
            //                Id = userMajorDetail.MajorDetail.Major.Id,
            //                Code = userMajorDetail.MajorDetail.Major.Code,
            //                Name = userMajorDetail.MajorDetail.Major.Name,
            //                MajorCode = userMajorDetail.MajorDetail.MajorCode,
            //                PositionOfUser = userMajorDetail.Rank?.Position,
            //                RankingMark = userMajorDetail.Rank?.TotalMark,
            //                TotalUserCared = (await _uow.UserMajorDetailRepository.Get(filter: u => u.MajorDetailId == userMajorDetail.MajorDetailId)).Count(),
            //                NumberOfStudent = userMajorDetail.MajorDetail.AdmissionCriteria.FirstOrDefault(a => a.Year == Consts.NEAREST_YEAR)?.Quantity,
            //                NewestEntryMark = userMajorDetail.MajorDetail.EntryMarks.FirstOrDefault(e => e.Year == Consts.NEAREST_YEAR
            //                                                                                    && e.SubjectGroupId == userMajorDetail.SubjectGroupId)?.Mark,
            //                YearOfEntryMark = Consts.NEAREST_YEAR,
            //                SubjectGroupId = userMajorDetail.SubjectGroupId,
            //                SubjectGroupCode = userMajorDetail.SubjectGroup.GroupCode,
            //            };
            //            majorGroupByTrainingProgramDataSets.Add(majorGroupByTrainingProgramDataSet);
            //        }
            //        trainingProgramGroupByUniversityDataSet.MajorGroupByTrainingProgramDataSets = majorGroupByTrainingProgramDataSets;
            //        trainingProgramGroupByUniversityDataSets.Add(trainingProgramGroupByUniversityDataSet);
            //    }
            //    userMajorDetailDataSet.TrainingProgramGroupByUniversityDataSets = trainingProgramGroupByUniversityDataSets;
            //    result.Add(userMajorDetailDataSet);
            //}
            //if (result.Count <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Bạn chưa có ngành quan tâm");
            //}
            //else
            //{
            //    response.Succeeded = true;
            //    response.Data = result;
            //}
            return response;
        }

        public async Task<Response<IEnumerable<RankingUserInformationGroupByRankType>>> GetUsersByMajorDetailId(RankingUserParam rankingUserParam)
        {
            Response<IEnumerable<RankingUserInformationGroupByRankType>> response = new Response<IEnumerable<RankingUserInformationGroupByRankType>>();
            //if (rankingUserParam == null || rankingUserParam.MajorId <= 0 || rankingUserParam.UniversityId <= 0 || rankingUserParam.TrainingProgramId <= 0)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Thông tin truy cập không hợp lệ!");
            //    return response;
            //}

            //MajorDetail majorDetail = await _uow.MajorDetailRepository.GetFirst(filter: m => m.UniversityId == rankingUserParam.UniversityId
            //                                                            && m.MajorId == rankingUserParam.MajorId
            //                                                            && m.TrainingProgramId == rankingUserParam.TrainingProgramId);
            //if (majorDetail == null)
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Thông tin truy cập không tồn tại!");
            //    return response;
            //}

            //IEnumerable<Models.UserMajorDetail> userMajorDetails = await _uow.UserMajorDetailRepository.Get(filter: u => u.MajorDetailId == majorDetail.Id,
            //                                                                                                    includeProperties: "SubjectGroup,User,Rank,Rank.RankType");
            //if (userMajorDetails == null || !userMajorDetails.Any())
            //{
            //    response.Succeeded = false;
            //    if (response.Errors == null)
            //    {
            //        response.Errors = new List<string>();
            //    }
            //    response.Errors.Add("Hiện tại chưa có lượt quan tâm nào!");
            //    return response;
            //}

            //IEnumerable<IGrouping<Models.Rank, Models.UserMajorDetail>> userMajorDetailsGroupsByRankType = userMajorDetails.GroupBy(u => u.Rank);

            //List<RankingUserInformationGroupByRankType> rankingUserInformationGroupByRanks = new List<RankingUserInformationGroupByRankType>();

            //foreach (IGrouping<Models.Rank, Models.UserMajorDetail> userMajorDetailsGroup in userMajorDetailsGroupsByRankType)
            //{
            //    List<RankingUserInformation> rankingUserInformations = new List<RankingUserInformation>();
            //    RankingUserInformationGroupByRankType rankingUserInformationGroupByRank = new RankingUserInformationGroupByRankType();
            //    foreach (Models.UserMajorDetail userMajorDetail in userMajorDetailsGroup)
            //    {
            //        RankingUserInformation rankingUserInformation = _mapper.Map<RankingUserInformation>(userMajorDetail.User);
            //        rankingUserInformation.GroupCode = userMajorDetail?.SubjectGroup.GroupCode;
            //        rankingUserInformation.Position = userMajorDetail?.Rank.Position;
            //        rankingUserInformation.TotalMark = userMajorDetail?.Rank.TotalMark;
            //        rankingUserInformations.Add(rankingUserInformation);
            //    }
            //    rankingUserInformationGroupByRank.Id = userMajorDetailsGroup.Key.RankTypeId;
            //    rankingUserInformationGroupByRank.Name = userMajorDetailsGroup.Key.RankType.Name;
            //    rankingUserInformationGroupByRank.RankingUserInformations = rankingUserInformations;
            //    rankingUserInformationGroupByRanks.Add(rankingUserInformationGroupByRank);
            //}

            //response.Succeeded = true;
            //response.Data = rankingUserInformationGroupByRanks;
            return response;
        }
    }
}
