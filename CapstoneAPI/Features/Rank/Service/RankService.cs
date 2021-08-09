using AutoMapper;
using CapstoneAPI.Features.FCM.Service;
using CapstoneAPI.Features.FollowingDetail.DataSet;
using CapstoneAPI.Features.Rank.DataSet;
using CapstoneAPI.Features.University.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Email;
using CapstoneAPI.Wrappers;
using FirebaseAdmin.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Rank.Service
{
    public class RankService : IRankService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly IFCMService _firebaseService;
        private readonly ILogger _log = Log.ForContext<RankService>();

        public RankService(IUnitOfWork uow, IMapper mapper, IEmailService emailService, IFCMService firebaseService)
        {
            _uow = uow;
            _mapper = mapper;
            _emailService = emailService;
            _firebaseService = firebaseService;
        }
        public async Task<Response<bool>> UpdateRank()
        {

            //CalculateRank là tính xếp hạng giữa các người dùng
            //CalculateNewRank là tính tổng điểm mới của người dùng
            Response<bool> response = new Response<bool>();
            try
            {
                Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
                Models.Season previousSeason = await _uow.SeasonRepository.GetPreviousSeason();

                if (currentSeason == null && previousSeason == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mùa chưa được kích hoạt!");
                    return response;
                }

                SeasonDataSet previousSeasonDataSet = new SeasonDataSet
                {
                    Id = previousSeason.Id,
                    Name = previousSeason.Name
                };
                List<Models.User> updateUsers = (await _uow.UserRepository
                    .Get(filter: u => u.Transcripts.Where(t => t.IsUpdate).Any(), includeProperties: "FollowingDetails.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail," +
                                                                            "FollowingDetails.EntryMark.MajorSubjectGroup.SubjectGroup.SubjectGroupDetails," +
                                                                            "FollowingDetails.Rank")).ToList();
                foreach (Models.User user in updateUsers.ToList())
                {
                    if (user.FollowingDetails == null || !user.FollowingDetails.Any())
                    {
                        updateUsers.Remove(user);
                        continue;
                    }
                    user.Transcripts = (await _uow.TranscriptRepository.Get(t => t.Status == Consts.STATUS_ACTIVE
                            && t.UserId == user.Id, includeProperties: "TranscriptType")).ToList();
                    foreach (Models.FollowingDetail followingDetail in user.FollowingDetails)
                    {
                        if (followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.SeasonId != currentSeason.Id)
                        {
                            continue;
                        }
                        var rankingFollowingDetailDataSet = _mapper.Map<RankFollowingDetailDataSet>(followingDetail);
                        await SetUpPreviousSeasonDataSet(rankingFollowingDetailDataSet, previousSeasonDataSet);
                        Models.Rank newRank = await CalculateNewRank(user.Transcripts.GroupBy(t => t.TranscriptType).ToList(),
                                                                    followingDetail.EntryMark.MajorSubjectGroup.SubjectGroup.SubjectGroupDetails.ToList(),
                                                                    followingDetail.Rank, (double) previousSeasonDataSet.EntryMark);
                        if (newRank != null && (newRank.TotalMark != followingDetail.Rank.TotalMark 
                            || followingDetail.Rank.TranscriptTypeId != newRank.TranscriptTypeId))
                        {
                            followingDetail.Rank.IsUpdate = true;
                            followingDetail.Rank.TranscriptTypeId = newRank.TranscriptTypeId;
                            followingDetail.Rank.UpdatedDate = JWTUtils.GetCurrentTimeInVN();
                            followingDetail.Rank.TotalMark = newRank.TotalMark;
                            _uow.RankRepository.Update(followingDetail.Rank);
                        }
                    }

                    foreach (Models.Transcript transcript in user.Transcripts)
                    {
                        if (transcript.IsUpdate)
                        {
                            transcript.IsUpdate = false;
                            transcript.DateRecord = JWTUtils.GetCurrentTimeInVN();
                        }
                        _uow.TranscriptRepository.Update(transcript);
                    }
                }

                await _uow.CommitAsync();

                IEnumerable<int> changedSubAdmissionIds = (await _uow.FollowingDetailRepository
                    .Get(filter: f => f.Rank.IsUpdate, includeProperties: "Rank,EntryMark"))
                    .Select(u => u.EntryMark.SubAdmissionCriterionId).Distinct();
                if (!changedSubAdmissionIds.Any())
                {
                    response.Succeeded = true;
                    response.Message = ("Không có thứ hạng thay đổi");
                    return response;
                }

                List<RankDataSet> emailedRankDataSets = new List<RankDataSet>();
                List<RankDataSet> updatedRankDataSets = new List<RankDataSet>();
                foreach (int subAdmissionId in changedSubAdmissionIds)
                {
                    IEnumerable<Models.FollowingDetail> sameSubAdmissionFollowingDetails = await _uow.FollowingDetailRepository
                        .Get(filter: u => u.EntryMark.SubAdmissionCriterionId == subAdmissionId && u.Status == Consts.STATUS_ACTIVE,
                        includeProperties: "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University," +
                                                                "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram," +
                                                                "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major," +
                                                                "EntryMark.MajorSubjectGroup.SubjectGroup,User,Rank");
                    if (!sameSubAdmissionFollowingDetails.Any())
                    {
                        continue;
                    }
                    var followingDetailsGroupByUser = sameSubAdmissionFollowingDetails
                        .OrderBy(s => s.Rank.Position).GroupBy(s => s.User);

                    sameSubAdmissionFollowingDetails = followingDetailsGroupByUser.Select(s => s.FirstOrDefault());

                    IEnumerable<RankDataSet> rankDataSets = sameSubAdmissionFollowingDetails.Select(f => _mapper.Map<RankDataSet>(f.Rank));
                    //Set up previous để tính lại rank
                    var rankingFollowingDetailDataSet = _mapper.Map<RankFollowingDetailDataSet>(sameSubAdmissionFollowingDetails.FirstOrDefault());
                    await SetUpPreviousSeasonDataSet(rankingFollowingDetailDataSet, previousSeasonDataSet);
                    foreach (RankDataSet rankDataSet in rankDataSets)
                    {
                        rankDataSet.NewPosition = _uow.RankRepository
                            .CalculateRank(rankDataSet.TranscriptTypeId, rankDataSet.TotalMark,
                            sameSubAdmissionFollowingDetails.Select(u => u.Rank), (double) previousSeasonDataSet.EntryMark);
                        if (rankDataSet.Position != rankDataSet.NewPosition)
                        {
                            emailedRankDataSets.Add(rankDataSet);
                        }
                        if (rankDataSet.Position != rankDataSet.NewPosition || rankDataSet.IsUpdate)
                        {
                            RankDataSet updateRankDataSet = new RankDataSet
                            {
                                IsUpdate = false,
                                Position = rankDataSet.NewPosition,
                                TotalMark = rankDataSet.TotalMark,
                                TranscriptTypeId = rankDataSet.TranscriptTypeId,
                                UpdatedDate = JWTUtils.GetCurrentTimeInVN(),
                                FollowingDetailId = rankDataSet.FollowingDetailId,
                                IsReceiveNotification = rankDataSet.IsReceiveNotification
                            };
                            updatedRankDataSets.Add(updateRankDataSet);
                        }
                    }
                }

                IEnumerable<Models.Rank> updateUnfollowRank = await _uow.RankRepository.Get(r => r.IsUpdate && r.FollowingDetail.Status == Consts.STATUS_INACTIVE);
                foreach(Models.Rank rank in updateUnfollowRank)
                {
                    rank.IsUpdate = false;
                    _uow.RankRepository.Update(rank);
                }

                foreach (RankDataSet updatedRankDataSet in updatedRankDataSets)
                {
                    Models.Rank rank = await _uow.RankRepository.GetById(updatedRankDataSet.FollowingDetailId);
                    rank.Position = updatedRankDataSet.Position;
                    rank.IsUpdate = updatedRankDataSet.IsUpdate;
                    _uow.RankRepository.Update(rank);
                }

                await _uow.CommitAsync();

                if (!emailedRankDataSets.Any())
                {
                    response.Succeeded = true;
                    response.Message = ("Không có thứ hạng thay đổi");
                    return response;
                }

                List<RankFollowingDetailDataSet> emailedFollowingDetails = new List<RankFollowingDetailDataSet>();
                foreach (RankDataSet rankDataSet in emailedRankDataSets)
                {
                    RankFollowingDetailDataSet rankFollowingDetailDataSet = _mapper.Map<RankFollowingDetailDataSet>(await _uow.FollowingDetailRepository
                                                                .GetFirst(filter: u => u.Id == rankDataSet.FollowingDetailId && u.Status == Consts.STATUS_ACTIVE,
                                                                includeProperties: "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University," +
                                                                "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram," +
                                                                "EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major," +
                                                                "EntryMark.MajorSubjectGroup.SubjectGroup,User,Rank"));
                    rankFollowingDetailDataSet.RankDataSet = rankDataSet;
                    emailedFollowingDetails.Add(rankFollowingDetailDataSet);
                }
                string message = @"<html>
                      <body>
                      <p>Xin chào {0},</p>
                      <p>Cảm ơn bạn đã tham gia vào hệ thống MOHS của chúng tôi.</p>
                      <p>Chúng tôi gửi đến bạn cập nhập thứ hạng của bạn trong hệ thống.</p>
                        {1}
                      <p>Xin cảm ơn, <br>MOHS</br></p>
                      </body>
                      </html>
                     ";
                string baseContent = @"<p>Trường: {0} - Ngành: {1} - Hệ đào tạo: {2}</p>
                                <p>Thứ hạng cũ: {3} - Thứ hạng mới: {4}</p>";
                string upNoti = @"Thứ hạng của bạn trong Trường {0} - Ngành {1} - Hệ {2} tăng từ top {3} lên top {4}";
                string downNoti = @"Thứ hạng của bạn trong Trường {0} - Ngành {1} - Hệ {2} giảm từ top {3} xuống top {4}";
                string outNoti = @"Điểm của bạn không còn phù hợp với Trường {0} - Ngành {1} - Hệ {2}";
                IEnumerable<IGrouping<Models.User, RankFollowingDetailDataSet>> emailedUserGroups 
                    = emailedFollowingDetails.GroupBy(u => u.User);
                List<Models.Notification> notifications = new List<Models.Notification>();
                var messages = new List<Message>();
                foreach (IGrouping<Models.User, RankFollowingDetailDataSet> emailedUserGroup in emailedUserGroups)
                {
                    string content = "";
                    foreach (RankFollowingDetailDataSet rankingFollowingDetailDataSet in emailedUserGroup)
                    {
                        content += string.Format(baseContent, rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University.Name,
                                                rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major.Name,
                                                rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram.Name,
                                                rankingFollowingDetailDataSet.RankDataSet.Position,
                                                rankingFollowingDetailDataSet.RankDataSet.NewPosition);
                        Models.Notification notification = new Models.Notification()
                        {
                            DateRecord = JWTUtils.GetCurrentTimeInVN(),
                            Data = rankingFollowingDetailDataSet.RankDataSet.FollowingDetailId.ToString(),
                            Message = string.Format(rankingFollowingDetailDataSet.RankDataSet.Position >
                                                rankingFollowingDetailDataSet.RankDataSet.NewPosition ? upNoti : downNoti,
                                                rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University.Name,
                                                rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major.Name,
                                                rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram.Name,
                                                rankingFollowingDetailDataSet.RankDataSet.Position,
                                                rankingFollowingDetailDataSet.RankDataSet.NewPosition),
                            IsRead = false,
                            Type = NotificationTypes.NewRank,
                            UserId = emailedUserGroup.Key.Id
                        };
                        notifications.Add(notification);
                        //Set up previous để thông báo out of uni
                        await SetUpPreviousSeasonDataSet(rankingFollowingDetailDataSet, previousSeasonDataSet);
                        if (rankingFollowingDetailDataSet.RankDataSet.TotalMark < previousSeasonDataSet.EntryMark)
                        {
                            Models.Notification outRankNoti = new Models.Notification()
                            {
                                DateRecord = JWTUtils.GetCurrentTimeInVN(),
                                Data = rankingFollowingDetailDataSet.RankDataSet.FollowingDetailId.ToString(),
                                Message = string.Format(outNoti,
                                    rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.University.Name,
                                    rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.Major.Name,
                                    rankingFollowingDetailDataSet.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.TrainingProgram.Name,
                                    rankingFollowingDetailDataSet.RankDataSet.Position,
                                    rankingFollowingDetailDataSet.RankDataSet.NewPosition),
                                IsRead = false,
                                Type = NotificationTypes.NewRank,
                                UserId = emailedUserGroup.Key.Id
                            };
                            messages.Add(new Message()
                            {
                                Notification = new FirebaseAdmin.Messaging.Notification()
                                {
                                    Title = "Không đủ điều kiện nhập học!",
                                    Body = outRankNoti.Message,
                                },
                                Data = new Dictionary<string, string>()
                                {
                                    {"type" , notification.Type.ToString()},
                                    {"message" , notification.Message},
                                    {"data" , outRankNoti.Data},
                                },
                                Topic = emailedUserGroup.Key.Id.ToString()
                            });
                            notifications.Add(outRankNoti);
                        }
                        
                        messages.Add(new Message()
                        {
                            Notification = new FirebaseAdmin.Messaging.Notification()
                            {
                                Title = "Thứ hạng của bạn đã được cập nhật!",
                                Body = notification.Message,
                            },
                            Data = new Dictionary<string, string>()
                            {
                                {"type" , notification.Type.ToString()},
                                {"message" , notification.Message},
                                {"data" , notification.Data},
                            },
                            Topic = emailedUserGroup.Key.Id.ToString()
                        });
                    }
                   
                    //string completedMessage = string.Format(message, emailedUserGroup.Key.Fullname, content);
                    #pragma warning disable
                    //_emailService.SendEmailAsync(emailedUserGroup.Key.Email, "MOHS RANK UPDATION", completedMessage);
                }
                _uow.NotificationRepository.InsertRange(notifications);
                await _uow.CommitAsync();
                _firebaseService.SendBatchMessage(messages);
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
                return response;
            }

            response.Succeeded = true;
            return response;
        }

        private async Task<Models.Rank> CalculateNewRank(List<IGrouping<TranscriptType, Models.Transcript>> transcriptGroup,
            List<SubjectGroupDetail> subjectGroupDetails, Models.Rank currentRank, double previousEntryMark)
        {
            double totalMark = 0;
            if (subjectGroupDetails == null || !subjectGroupDetails.Any())
            {
                return null;
            }

            Models.Rank newRank = new Models.Rank();

            foreach (IGrouping<TranscriptType, Models.Transcript> group in transcriptGroup.OrderByDescending(t => t.Key.Priority))
            {
                foreach (SubjectGroupDetail subjectGroupDetail in subjectGroupDetails)
                {
                    if (subjectGroupDetail.SpecialSubjectGroupId == null && subjectGroupDetail.SubjectId == null)
                    {
                        return null;
                    }

                    if (subjectGroupDetail.SubjectId != null)
                    {
                        Models.Transcript transcript = group.FirstOrDefault(m => m.SubjectId == subjectGroupDetail.SubjectId && m.Status == Consts.STATUS_ACTIVE);
                        if (transcript == null || transcript.Mark < 0)
                        {
                            totalMark += 0;
                        }
                        else
                        {
                            totalMark += transcript.Mark;
                        }
                    }
                    else
                    {
                        double totalSpecialGroupMark = 0;
                        IEnumerable<Models.Subject> subjects = (await _uow.SubjectRepository.Get(s => s.SpecialSubjectGroupId == subjectGroupDetail.SpecialSubjectGroupId));

                        if (!subjects.Any())
                        {
                            return null;
                        }

                        foreach (Models.Subject subject in subjects)
                        {
                            Models.Transcript transcript = group.FirstOrDefault(m => m.SubjectId == subject.Id && m.Status == Consts.STATUS_ACTIVE);
                            if (transcript == null || transcript.Mark < 0)
                            {
                                totalSpecialGroupMark += 0;
                            }
                            else
                            {
                                totalSpecialGroupMark += transcript.Mark;
                            }
                        }

                        totalMark += (totalSpecialGroupMark / subjects.Count());
                    }
                }
                if (group.Key.Id == currentRank.TranscriptTypeId || totalMark >= previousEntryMark)
                {
                    newRank.TotalMark = totalMark;
                    newRank.TranscriptTypeId = group.Key.Id;
                    return newRank;
                }
            }
            return null;
        }


        private async Task SetUpPreviousSeasonDataSet(RankFollowingDetailDataSet followingDetail, SeasonDataSet previousSeasonDataSet)
        {
            MajorDetail previousMajorDetail = await _uow.MajorDetailRepository.GetFirst(filter: m => m.SeasonId == previousSeasonDataSet.Id
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
