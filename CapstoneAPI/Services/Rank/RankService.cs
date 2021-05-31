using AutoMapper;
using CapstoneAPI.DataSets.Rank;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Services.Email;
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
        public RankService(IUnitOfWork uow, IMapper mapper, IEmailService emailService)
        {
            _uow = uow;
            _mapper = mapper;
            _emailService = emailService;
        }
        public async Task<bool> UpdateRank()
        {
            //IEnumerable<int> changedMajorDetailIds = (await _uow.FollowingDetailRepository
            //                                            .Get(includeProperties: "Rank")).Where(u => u.Rank != null && u.Rank.IsNew)
            //                                            .Select(u => u.MajorDetailId).Distinct();
            //if (!changedMajorDetailIds.Any())
            //{
            //    return false;
            //}

            //List<RankDataSet> emailedRankDataSets = new List<RankDataSet>();
            //List<RankDataSet> updatedRankDataSets = new List<RankDataSet>();
            //foreach (int majorDetailId in changedMajorDetailIds)
            //{
            //    IEnumerable<Models.UserMajorDetail> sameMajorDetails = await _uow.FollowingDetailRepository
            //                                                        .Get(filter: u => u.MajorDetailId == majorDetailId, includeProperties: "Rank,User");
            //    IEnumerable<RankDataSet> rankDataSets = sameMajorDetails.Select(u => _mapper.Map<RankDataSet>(u.Rank));
            //    foreach (RankDataSet rankDataSet in rankDataSets)
            //    {
            //        rankDataSet.NewPosition = _uow.RankRepository.CalculateRank(rankDataSet.RankTypeId, rankDataSet.TotalMark, sameMajorDetails.Select(u => u.Rank));
            //        if (rankDataSet.Position != rankDataSet.NewPosition)
            //        {
            //            emailedRankDataSets.Add(rankDataSet);
            //        }
            //        if (rankDataSet.Position != rankDataSet.NewPosition || rankDataSet.IsNew)
            //        {
            //            RankDataSet updateRankDataSet = new RankDataSet
            //            {
            //                IsNew = false,
            //                Position = rankDataSet.NewPosition,
            //                TotalMark = rankDataSet.TotalMark,
            //                RankTypeId = rankDataSet.RankTypeId,
            //                UpdatedDate = DateTime.UtcNow,
            //                UserMajorDetailId = rankDataSet.UserMajorDetailId,
            //                IsReceiveNotification = rankDataSet.IsReceiveNotification
            //            };
            //            updatedRankDataSets.Add(updateRankDataSet);
            //        }
            //    }
            //}

            //if (!emailedRankDataSets.Any())
            //{
            //    return false;
            //}

            //List<UserMajorDetailDataSet> emailedUserMajorDetails = new List<UserMajorDetailDataSet>();
            //foreach(RankDataSet rankDataSet in emailedRankDataSets)
            //{
            //    UserMajorDetailDataSet userMajorDetailDataSet = _mapper.Map<UserMajorDetailDataSet>(await _uow.FollowingDetailRepository
            //                                                .GetFirst(filter: u => u.Id == rankDataSet.UserMajorDetailId, includeProperties: "MajorDetail,User,Rank,MajorDetail.Major,MajorDetail.University,MajorDetail.TrainingProgram"));
            //    userMajorDetailDataSet.Rank = rankDataSet;
            //    emailedUserMajorDetails.Add(userMajorDetailDataSet);
            //}
            //string message = @"<html>
            //          <body>
            //          <p>Xin chào {0},</p>
            //          <p>Cảm ơn bạn đã tham gia vào hệ thống MOHS của chúng tôi.</p>
            //          <p>Chúng tôi gửi đến bạn cập nhập thứ hạng của bạn trong hệ thống.</p>
            //            {1}
            //          <p>Xin cảm ơn, <br>MOHS</br></p>
            //          </body>
            //          </html>
            //         ";
            //string baseContent = @"<p>Trường: {0} - Ngành: {1} - Hệ đào tạo: {2}</p>
            //                    <p>Thứ hạng cũ: {3} - Thứ hạng mới: {4}</p>";
            //IEnumerable<IGrouping<Models.User, UserMajorDetailDataSet>> emailedUserGroups = emailedUserMajorDetails.GroupBy(u => u.User);
            //foreach(IGrouping<Models.User, UserMajorDetailDataSet> emailedUserGroup in emailedUserGroups)
            //{
            //    string content = "";
            //    foreach (UserMajorDetailDataSet userMajorDetailDataSet in emailedUserGroup)
            //    {
            //        content += string.Format(baseContent, userMajorDetailDataSet.MajorDetail.University.Name,
            //                                userMajorDetailDataSet.MajorDetail.Major.Name,
            //                                userMajorDetailDataSet.MajorDetail.TrainingProgram.Name,
            //                                userMajorDetailDataSet.Rank.Position,
            //                                userMajorDetailDataSet.Rank.NewPosition);
            //    }
            //    string completedMessage = string.Format(message, emailedUserGroup.Key.Fullname, content);
            //    await _emailService.SendEmailAsync(emailedUserGroup.Key.Email, "MOHS RANK UPDATION", completedMessage);
            //}

            //foreach (RankDataSet updatedRankDataSet in updatedRankDataSets)
            //{
            //    Models.Rank rank = await _uow.RankRepository.GetById(updatedRankDataSet.UserMajorDetailId);
            //    rank.Position = updatedRankDataSet.Position;
            //    rank.IsNew = updatedRankDataSet.IsNew;
            //    _uow.RankRepository.Update(rank);
            //}

            return (await _uow.CommitAsync() > 0);
        }
    }
}
