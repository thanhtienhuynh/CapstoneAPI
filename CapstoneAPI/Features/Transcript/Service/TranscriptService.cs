using AutoMapper;
using CapstoneAPI.Features.FCM.Service;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Features.Transcript.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using FirebaseAdmin.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Transcript.Service
{
    public class TranscriptService :ITranscriptService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _log = Log.ForContext<TranscriptService>();
        private readonly IFCMService _firebaseService;

        public TranscriptService(IUnitOfWork uow, IMapper mapper, IFCMService firebaseService)
        {
            _uow = uow;
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        public async Task<Response<IEnumerable<UserTranscriptTypeDataSet>>> GetMarkOfUser(string token)
        {
            Response<IEnumerable<UserTranscriptTypeDataSet>> response = new Response<IEnumerable<UserTranscriptTypeDataSet>>();
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
                IEnumerable<UserTranscriptTypeDataSet> result = await _uow.TranscriptRepository.GetUserTranscripts(user.Id);
                response.Succeeded = true;
                response.Data = result;
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

        public async Task<Response<bool>> SaveMarkOfUser(string token, SubjectGroupParam subjectGroupParam)
        {
            Response<bool> response = new Response<bool>();
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
                List<Models.Transcript> newTranscripts = new List<Models.Transcript>();
                foreach (MarkParam markParam in subjectGroupParam.Marks)
                {
                    int transcriptTypeId = subjectGroupParam.TranscriptTypeId;
                    IEnumerable<Models.Transcript> transcripts = await _uow.TranscriptRepository
                                            .Get(filter: t => t.SubjectId == markParam.SubjectId
                                                        && t.UserId == user.Id
                                                        && t.TranscriptTypeId == transcriptTypeId && t.Status == Consts.STATUS_ACTIVE);
                    newTranscripts.Add(new Models.Transcript()
                    {
                        Mark = markParam.Mark,
                        SubjectId = markParam.SubjectId,
                        UserId = user.Id,
                        TranscriptTypeId = subjectGroupParam.TranscriptTypeId,
                        DateRecord = DateTime.UtcNow,
                        IsUpdate = true,
                        Status = Consts.STATUS_ACTIVE
                    });

                    if (transcripts.Any())
                    {
                        foreach (Models.Transcript transcript in transcripts)
                        {
                            transcript.Status = Consts.STATUS_INACTIVE;
                            transcript.DateRecord = DateTime.UtcNow;
                        }
                        _uow.TranscriptRepository.UpdateRange(transcripts);
                    }
                }

                _uow.TranscriptRepository.InsertRange(newTranscripts);
                var messages = new List<Message>();

                if (user.Gender != subjectGroupParam.Gender || user.ProvinceId != subjectGroupParam.ProvinceId)
                {
                    IEnumerable<Models.FollowingDetail> followingDetails = await _uow.FollowingDetailRepository.Get(
                        filter: f => f.UserId == user.Id && f.Status == Consts.STATUS_ACTIVE
                               && f.EntryMark.SubAdmissionCriterion.Gender == user.Gender
                               && f.EntryMark.SubAdmissionCriterion.ProvinceId == user.ProvinceId,
                        includeProperties: "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University," +
                        "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major," +
                        "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram");
                    List<Models.Notification> notifications = new List<Models.Notification>();
                    string message = @"Bạn không còn phụ hợp với tiêu chí tuyển sinh của Ngành {0} - Hệ {1} - Trường {2} do đã thay đổi thông tin!";
                    foreach (Models.FollowingDetail followingDetail in followingDetails)
                    {
                        followingDetail.Status = Consts.STATUS_INACTIVE;
                        Models.Notification notification = new Models.Notification()
                        {
                            Data = followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion
                                    .MajorDetail.UniversityId.ToString(),
                            DateRecord = DateTime.UtcNow,
                            IsRead = false,
                            Message = string.Format(message, followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major,
                                followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram,
                                followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University),
                            Type = 6,
                            UserId = user.Id
                        };
                        notifications.Add(notification);
                        messages.Add(new Message()
                        {
                            Notification = new FirebaseAdmin.Messaging.Notification()
                            {
                                Title = "Thông tin theo dõi đã được cập nhật!",
                                Body = notification.Message,
                            },
                            Data = new Dictionary<string, string>()
                            {
                                {"type" , notification.Type.ToString()},
                                {"message" , notification.Message},
                                {"data" , notification.Data},
                            },
                            Topic = user.Id.ToString()
                        });
                    }
                    user.Gender = subjectGroupParam.Gender;
                    user.ProvinceId = subjectGroupParam.ProvinceId;
                    _uow.UserRepository.Update(user);
                    _uow.FollowingDetailRepository.UpdateRange(followingDetails);
                    _uow.NotificationRepository.InsertRange(notifications);
                }

                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lưu thông tin không thành công!");
                    return response;
                }
                response.Succeeded = true;
                response.Data = true;
                if (messages.Any())
                {
                    #pragma warning disable
                    _firebaseService.SendBatchMessage(messages);
                }
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

        public async Task<Response<bool>> SaveSingleTranscript(string token, TranscriptParam transcriptParam)
        {
            Response<bool> response = new Response<bool>();
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

                IEnumerable<Models.Transcript> transcripts = await _uow.TranscriptRepository
                                            .Get(filter: t => t.SubjectId == transcriptParam.SubjectId
                                                        && t.UserId == user.Id
                                                        && t.TranscriptTypeId == transcriptParam.TranscriptTypeId 
                                                        && t.Status == Consts.STATUS_ACTIVE);
                if (transcripts.Any())
                {
                    foreach (Models.Transcript transcript in transcripts)
                    {
                        transcript.Status = Consts.STATUS_INACTIVE;
                        transcript.DateRecord = DateTime.UtcNow;
                    }
                    _uow.TranscriptRepository.UpdateRange(transcripts);
                }

                Models.Transcript newTranscript = new Models.Transcript()
                {
                    Mark = transcriptParam.Mark,
                    SubjectId = transcriptParam.SubjectId,
                    UserId = user.Id,
                    TranscriptTypeId = transcriptParam.TranscriptTypeId,
                    DateRecord = DateTime.UtcNow,
                    IsUpdate = true,
                    Status = Consts.STATUS_ACTIVE
                };

                _uow.TranscriptRepository.Insert(newTranscript);

                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Lưu điểm không thành công!");
                    return response;
                }
                response.Succeeded = true;
                response.Data = true;
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
    }
}
