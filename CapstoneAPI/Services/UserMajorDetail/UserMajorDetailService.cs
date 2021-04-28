using AutoMapper;
using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.DataSets.UserMajorDetail;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
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

        public async Task<Models.UserMajorDetail> AddUserMajorDetail(AddUserMajorDetailParam userMajorDetailParam, string token)
        {
            if (token == null || token.Trim().Length == 0)
            {
                return null;
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Length <= 0)
            {
                return null;
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
                return null;
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
                    return null;
                }
            }

            return userMajorDetail;
        }

        public async Task<BaseResponse<Object>> RemoveUserMajorDetail(UpdateUserMajorDetailParam userMajorDetailParam, string token)
        {
            BaseResponse<Object> response;
            if (token == null || token.Trim().Length == 0)
            {
                return response = new BaseResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = 1,
                    Message = "Bạn chưa đăng nhập"
                };
            }

            string userIdString = JWTUtils.GetUserIdFromJwtToken(token);

            if (userIdString == null || userIdString.Length <= 0)
            {
                return response = new BaseResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = 1,
                    Message = "Bạn chưa đăng nhập"
                };
            }

            int userId = Int32.Parse(userIdString);

            MajorDetail majorDetail = await _uow.MajorDetailRepository
                                        .GetFirst(filter: m => m.MajorId == userMajorDetailParam.MajorId
                                                && m.TrainingProgramId == userMajorDetailParam.TrainingProgramId
                                                && m.UniversityId == userMajorDetailParam.UniversityId);

            if (majorDetail == null)
            {
                return response = new BaseResponse<object>()
                {
                    IsSuccess = false,
                    StatusCode = 2,
                    Message = "Trường này không tồn tại"
                };
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
                        return response = new BaseResponse<object>()
                        {
                            IsSuccess = false,
                            StatusCode = 3,
                            Message = "Lỗi hệ thống"
                        };
                    }
                }

                _uow.UserMajorDetailRepository.Delete(userMajorDetail.Id);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    return response = new BaseResponse<object>()
                    {
                        IsSuccess = false,
                        StatusCode = 3,
                        Message = "Lỗi hệ thống"
                    };
                }
            }

            return response = new BaseResponse<object>()
            {
                IsSuccess = true,
                StatusCode = 0,
                Message = "Bỏ quan tâm thành công"
            };
        }
    }
}
