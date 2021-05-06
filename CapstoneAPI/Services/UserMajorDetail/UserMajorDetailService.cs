using AutoMapper;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.DataSets.UserMajorDetail;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.UserMajorDetail
{
    public class UserMajorDetailService : IUserMajorDetailService
    {

        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public UserMajorDetailService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<Models.UserMajorDetail>> AddUserMajorDetail(AddUserMajorDetailParam userMajorDetailParam, string token)
        {
            Response<Models.UserMajorDetail> response = new Response<Models.UserMajorDetail>();
            if (token == null || token.Trim().Length == 0)
            {
                response.Succeeded = false;
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Length <= 0)
            {
                response.Succeeded = false;
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
                    } else
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
                                                && m.UniversityId == userMajorDetailParam.UniversityId);

            if (majorDetail == null)
            {
                response.Succeeded = false;
                response.Errors.Add("Trường này không tồn tại!");
                return response;
            }

            Models.UserMajorDetail userMajorDetail = await _uow.UserMajorDetailRepository
                                                        .GetFirst(filter: u => u.UserId == userId
                                                                    && u.MajorDetailId == majorDetail.Id);
            if (userMajorDetail == null)
            {
                IEnumerable<Models.Rank> ranks = (await _uow.UserMajorDetailRepository
                                                            .Get(filter: u => u.MajorDetailId == majorDetail.Id, includeProperties: "Rank"))
                                                            .Select(u => u.Rank).Where(r => r != null);
                userMajorDetail = new Models.UserMajorDetail()
                {
                    MajorDetailId = majorDetail.Id,
                    UserId = userId,
                    SubjectGroupId = userMajorDetailParam.SubjectGroupId,
                    Status = Consts.STATUS_ACTIVE,
                    Rank = new Models.Rank()
                    {
                        IsNew = true,
                        IsReceiveNotification = true,
                        RankTypeId = userMajorDetailParam.SubjectGroupParam.TranscriptTypeId,
                        TotalMark = userMajorDetailParam.TotalMark,
                        UpdatedDate = DateTime.UtcNow,
                        Position = _uow.RankRepository.CalculateRank(userMajorDetailParam.SubjectGroupParam.TranscriptTypeId,
                                                                    userMajorDetailParam.TotalMark, ranks)
                    }
                };
                _uow.UserMajorDetailRepository.Insert(userMajorDetail);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    response.Errors.Add("Quan tâm không thành công, lỗi hệ thống!");
                    return response;
                }
            }
            response.Succeeded = true;
            response.Data = userMajorDetail;
            return response;
        }

        public async Task<Response<Object>> RemoveUserMajorDetail(UpdateUserMajorDetailParam userMajorDetailParam, string token)
        {
            Response<Object> response = new Response<Object>();
            if (token == null || token.Trim().Length == 0)
            {
                response.Succeeded = false;
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Length <= 0)
            {
                response.Succeeded = false;
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
                response.Errors.Add("Trường này không tồn tại!");
                return response;
            }

            Models.UserMajorDetail userMajorDetail = await _uow.UserMajorDetailRepository
                                                        .GetFirst(filter: u => u.UserId == userId
                                                                    && u.MajorDetailId == majorDetail.Id);
            if (userMajorDetail != null)
            {
                Models.Rank rank = await _uow.RankRepository.GetById(userMajorDetail.Id);
                if (rank != null)
                {
                    _uow.RankRepository.Delete(userMajorDetail.Id);
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        response.Errors.Add("Bỏ quan tâm không thành công, lỗi hệ thống!");
                        return response;
                    }
                }

                _uow.UserMajorDetailRepository.Delete(userMajorDetail.Id);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    response.Errors.Add("Bỏ quan tâm không thành công, lỗi hệ thống!");
                    return response;
                }
            }

            response.Succeeded = true;

            return response;
        }

        public async Task<Response<IEnumerable<UserMajorDetailGroupByMajorDataSet>>> GetUserMajorDetailGroupByMajorDataSets(string token)
        {
            Response<IEnumerable<UserMajorDetailGroupByMajorDataSet>> response = new Response<IEnumerable<UserMajorDetailGroupByMajorDataSet>>();

            
            if (token == null || token.Trim().Length == 0)
            {
                response.Succeeded = false;
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Length <= 0)
            {
                response.Succeeded = false;
                response.Errors.Add("Tài khoản của bạn không tồn tại!");
                return response;
            }

            int userId = Int32.Parse(userIdString);

            IEnumerable<Models.UserMajorDetail> userMajorDetails = await _uow.UserMajorDetailRepository.
                                Get(filter: u => u.UserId == userId,
                                includeProperties: "MajorDetail,Rank,MajorDetail.Major,MajorDetail.University,SubjectGroup,MajorDetail.AdmissionCriteria,MajorDetail.EntryMarks");
            IEnumerable<IGrouping<Models.Major, Models.UserMajorDetail>> userMajorDetailGroups = userMajorDetails.GroupBy(u => u.MajorDetail.Major);

            List<UserMajorDetailGroupByMajorDataSet> result = new List<UserMajorDetailGroupByMajorDataSet>();
            foreach (IGrouping<Models.Major,Models.UserMajorDetail> userMajorDetailGroup in userMajorDetailGroups)
            {                
                List<UserMajorDetailToReturn> detailOfDataSets = new List<UserMajorDetailToReturn>();
                foreach (Models.UserMajorDetail item in userMajorDetailGroup)
                {
                    UserMajorDetailToReturn userMajorDetailToReturn = new UserMajorDetailToReturn
                    {
                        PositionOfUser = item.Rank.Position,
                        TotalUserCared = _uow.UserMajorDetailRepository.Count(filter: u => u.MajorDetailId == item.MajorDetailId),

                        UniversityId = item.MajorDetail.University.Id,
                        University = new DataSets.University.CreateUniversityDataset
                        {
                            Name = item.MajorDetail.University.Name,
                            Code = item.MajorDetail.University.Code,
                            Address = item.MajorDetail.University.Address,
                            Phone = item.MajorDetail.University.Phone,
                            WebUrl = item.MajorDetail.University.WebUrl,
                            Rating = item.MajorDetail.University.Rating,
                            TuitionType = item.MajorDetail.University.TuitionType,
                            TuitionFrom = item.MajorDetail.University.TuitionFrom,
                            TuitionTo = item.MajorDetail.University.TuitionTo,
                            LogoUrl = item.MajorDetail.University.LogoUrl,
                            Description = item.MajorDetail.University.Description,
                        },
                        UniversityMajorCode = item.MajorDetail.MajorCode,
                        TrainingProgramId = item.MajorDetail.TrainingProgram.Id,
                        TrainingProgramName = item.MajorDetail.TrainingProgram.Name,
                        NumberOfStudent = item.MajorDetail.AdmissionCriteria.FirstOrDefault(u => u.MajorDetailId == item.MajorDetailId
                        && u.Year == Consts.NEAREST_YEAR).Quantity,
                        NewestEntryMark = item.MajorDetail.EntryMarks.FirstOrDefault(n => n.MajorDetailId == item.MajorDetailId
                        && n.Year == Consts.NEAREST_YEAR).Mark,
                        YearOfEntryMark = Consts.NEAREST_YEAR,

                        SubjectGroupId = item.SubjectGroup.Id,
                        SubjectGroupCode = item.SubjectGroup.GroupCode,
                    };
                    detailOfDataSets.Add(userMajorDetailToReturn);
                }
                UserMajorDetailGroupByMajorDataSet userMajorDetailDataSet = new UserMajorDetailGroupByMajorDataSet
                {
                    MajorId = userMajorDetailGroup.Key.Id,
                    MajorName = userMajorDetailGroup.Key.Name,
                    DetailOfDataSets = detailOfDataSets
                };
                result.Add(userMajorDetailDataSet);
            }
            if (result.Count<=0)
            {
                response.Succeeded = false;
                response.Errors.Add("Bạn chưa có ngành quan tâm");
                return response;
            }
            else
            {
                response.Succeeded = true;
                response.Data = result;
            }
            return response;
        }

        public async Task<Response<IEnumerable<UserMajorDetailGroupByUniversityDataSet>>> GetUserMajorDetailGroupByUniversityDataSets(string token)
        {
            Response<IEnumerable<UserMajorDetailGroupByUniversityDataSet>> response = new Response<IEnumerable<UserMajorDetailGroupByUniversityDataSet>>();

            
            if (token == null || token.Trim().Length == 0)
            {
                response.Succeeded = false;
                response.Errors.Add("Bạn chưa đăng nhập!");
                return response;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Length <= 0)
            {
                response.Succeeded = false;
                response.Errors.Add("Tài khoản của bạn không tồn tại!");
                return response;
            }

            int userId = Int32.Parse(userIdString);
            IEnumerable<Models.UserMajorDetail> userMajorDetails = await _uow.UserMajorDetailRepository.
                                Get(filter: u => u.UserId == userId,
                                includeProperties: "MajorDetail,Rank,MajorDetail.Major,MajorDetail.University,SubjectGroup,MajorDetail.AdmissionCriteria,MajorDetail.EntryMarks");
            IEnumerable<IGrouping<Models.University, Models.UserMajorDetail>> userMajorDetailGroups = userMajorDetails.GroupBy(u => u.MajorDetail.University);

            List<UserMajorDetailGroupByUniversityDataSet> result = new List<UserMajorDetailGroupByUniversityDataSet>();
            foreach (IGrouping<Models.University, Models.UserMajorDetail> userMajorDetailGroup in userMajorDetailGroups)
            {
                UserMajorDetailGroupByUniversityDataSet userMajorDetailDataSet = new UserMajorDetailGroupByUniversityDataSet
                {
                    UniversityId = userMajorDetailGroup.Key.Id,
                    University = new DataSets.University.CreateUniversityDataset
                    {
                        Code = userMajorDetailGroup.Key.Code,
                        Name = userMajorDetailGroup.Key.Name,
                        Description = userMajorDetailGroup.Key.Description,
                        Address = userMajorDetailGroup.Key.Address,
                        LogoUrl = userMajorDetailGroup.Key.LogoUrl,
                        Phone = userMajorDetailGroup.Key.Phone,
                        WebUrl = userMajorDetailGroup.Key.WebUrl,
                        TuitionTo = userMajorDetailGroup.Key.TuitionTo,
                        TuitionFrom = userMajorDetailGroup.Key.TuitionFrom,
                        TuitionType = userMajorDetailGroup.Key.TuitionType,
                        Rating = userMajorDetailGroup.Key.Rating
                    }
                };
                List<UserMajorDetailGroupByUniversityToReturn> detailOfDataSets = new List<UserMajorDetailGroupByUniversityToReturn>();
                foreach (Models.UserMajorDetail item in userMajorDetailGroup)
                {
                    UserMajorDetailGroupByUniversityToReturn detailOfDataSet = new UserMajorDetailGroupByUniversityToReturn
                    {
                        MajorId = (int)item.MajorDetail.MajorId,
                        MajorName = item.MajorDetail.Major.Name,
                        UniversityMajorCode = item.MajorDetail.MajorCode,
                        TrainingProgramId = item.MajorDetail.TrainingProgram.Id,
                        TrainingProgramName = item.MajorDetail.TrainingProgram.Name,
                        PositionOfUser = item.Rank.Position,
                        TotalUserCared = _uow.UserMajorDetailRepository.Count(filter: u => u.MajorDetailId == item.MajorDetailId),

                        NumberOfStudent = item.MajorDetail.AdmissionCriteria.FirstOrDefault(u => u.MajorDetailId == item.MajorDetailId
                        && u.Year == Consts.NEAREST_YEAR).Quantity,
                        NewestEntryMark = item.MajorDetail.EntryMarks.FirstOrDefault(n => n.MajorDetailId == item.MajorDetailId
                        && n.Year == Consts.NEAREST_YEAR).Mark,
                        YearOfEntryMark = Consts.NEAREST_YEAR,
                        
                        SubjectGroupId = item.SubjectGroup.Id,
                        SubjectGroupCode = item.SubjectGroup.GroupCode,
                    };
                    detailOfDataSets.Add(detailOfDataSet);
                }
                userMajorDetailDataSet.DetailOfDataSets = detailOfDataSets;
                result.Add(userMajorDetailDataSet);
            }
            if (result.Count <= 0)
            {
                response.Succeeded = false;
                response.Errors.Add("Bạn chưa có ngành quan tâm");
                return response;
            }
            else
            {
                response.Succeeded = true;
                response.Data = result;
            }
            return response;
        }
    }
}
