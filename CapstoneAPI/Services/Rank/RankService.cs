using AutoMapper;
using CapstoneAPI.DataSets.FollowingDetail;
using CapstoneAPI.DataSets.Rank;
using CapstoneAPI.DataSets.SubjectGroup;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Email;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Rank
{
    public class RankService : IRankService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly ILogger _log = Log.ForContext<RankService>();

        public RankService(IUnitOfWork uow, IMapper mapper, IEmailService emailService)
        {
            _uow = uow;
            _mapper = mapper;
            _emailService = emailService;
        }
        public async Task<bool> UpdateRank()
        {
            try
            {
                Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
                if (currentSeason == null)
                {
                    return false;
                }
                List<Models.User> updateUsers = (await _uow.UserRepository
                    .Get(filter: u => u.Transcripts.Where(t => t.IsUpdate).Any(), includeProperties: "FollowingDetails.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail," +
                                                                            "FollowingDetails.EntryMark.MajorSubjectGroup.SubjectGroup.SubjectGroupDetails," +
                                                                            "Transcripts.TranscriptType,FollowingDetails.Rank")).ToList();

                foreach (Models.User user in updateUsers.ToList())
                {
                    if (user.FollowingDetails == null || !user.FollowingDetails.Any())
                    {
                        updateUsers.Remove(user);
                        continue;
                    }

                    foreach (Models.FollowingDetail followingDetail in user.FollowingDetails)
                    {
                        if (followingDetail.EntryMark.SubAdmissionCriterion.AdmissionCriterion.MajorDetail.SeasonId != currentSeason.Id)
                            continue;
                        Models.Rank newRank = await CalculateNewRank(user.Transcripts.GroupBy(t => t.TranscriptType).ToList(),
                                                                    followingDetail.EntryMark.MajorSubjectGroup.SubjectGroup.SubjectGroupDetails.ToList(),
                                                                    followingDetail.Rank, followingDetail.EntryMark.Mark ?? 0);
                        if (newRank != null && (newRank.TotalMark != followingDetail.Rank.TotalMark || followingDetail.Rank.TranscriptTypeId != newRank.TranscriptTypeId))
                        {
                            followingDetail.Rank.IsUpdate = true;
                            followingDetail.Rank.TranscriptTypeId = newRank.TranscriptTypeId;
                            followingDetail.Rank.UpdatedDate = DateTime.UtcNow;
                            followingDetail.Rank.TotalMark = newRank.TotalMark;
                            _uow.RankRepository.Update(followingDetail.Rank);
                        }
                    }

                    foreach (Models.Transcript transcript in user.Transcripts)
                    {
                        if (transcript.IsUpdate)
                        {
                            transcript.IsUpdate = false;
                            transcript.DateRecord = DateTime.UtcNow;
                        }
                        _uow.TranscriptRepository.Update(transcript);

                    }
                }

                await _uow.CommitAsync();

                IEnumerable<int> changedSubAdmissionIds = (await _uow.FollowingDetailRepository
                                                            .Get(includeProperties: "Rank,EntryMark"))
                                                            .Where(u => u.Rank != null && u.Rank.IsUpdate)
                                                            .Select(u => u.EntryMark.SubAdmissionCriterionId).Distinct();
                if (!changedSubAdmissionIds.Any())
                {
                    return false;
                }

                List<RankDataSet> emailedRankDataSets = new List<RankDataSet>();
                List<RankDataSet> updatedRankDataSets = new List<RankDataSet>();
                foreach (int subAdmissionId in changedSubAdmissionIds)
                {
                    IEnumerable<Models.FollowingDetail> sameSubAdmissionFollowingDetails = await _uow.FollowingDetailRepository
                                                                        .Get(filter: u => u.EntryMark.SubAdmissionCriterionId == subAdmissionId && u.Status == Consts.STATUS_ACTIVE, includeProperties: "Rank,User");
                    IEnumerable<RankDataSet> rankDataSets = sameSubAdmissionFollowingDetails.Select(u => _mapper.Map<RankDataSet>(u.Rank));
                    foreach (RankDataSet rankDataSet in rankDataSets)
                    {
                        rankDataSet.NewPosition = _uow.RankRepository.CalculateRank(rankDataSet.TranscriptTypeId, rankDataSet.TotalMark, sameSubAdmissionFollowingDetails.Select(u => u.Rank));
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
                                UpdatedDate = DateTime.UtcNow,
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
                    return false;
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
                IEnumerable<IGrouping<Models.User, RankFollowingDetailDataSet>> emailedUserGroups = emailedFollowingDetails.GroupBy(u => u.User);
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
                    }
                    string completedMessage = string.Format(message, emailedUserGroup.Key.Fullname, content);
                    _emailService.SendEmailAsync(emailedUserGroup.Key.Email, "MOHS RANK UPDATION", completedMessage);
                }
            } catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return false;
            }

            return true;
        }

        private async Task<Models.Rank> CalculateNewRank(List<IGrouping<TranscriptType, Models.Transcript>> transcriptGroup, List<SubjectGroupDetail> subjectGroupDetails, Models.Rank currentRank, double entryMark)
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
                        Models.Transcript transcript = group.FirstOrDefault(m => m.SubjectId == subjectGroupDetail.SubjectId);
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
                            Models.Transcript transcript = group.FirstOrDefault(m => m.SubjectId == subject.Id);
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
                if (group.Key.Id == currentRank.TranscriptTypeId || totalMark >= entryMark)
                {
                    newRank.TotalMark = totalMark;
                    newRank.TranscriptTypeId = group.Key.Id;
                    return newRank;
                }
            }
            return null;
        }
    }
}
