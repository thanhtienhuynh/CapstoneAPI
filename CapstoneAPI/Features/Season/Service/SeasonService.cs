using AutoMapper;
using CapstoneAPI.DataSets;
using CapstoneAPI.Features.Season.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Season.Service
{
    public class SeasonService : ISeasonService
    {
        private readonly IUnitOfWork _uow;
        private IMapper _mapper;
        private readonly ILogger _log = Log.ForContext<SeasonService>();

        public SeasonService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<AdminSeasonDataSet>>> GetAllSeasons()
        {
            Response<IEnumerable<AdminSeasonDataSet>> response = new Response<IEnumerable<AdminSeasonDataSet>>();
            try
            {
                IEnumerable<AdminSeasonDataSet> seasons = (await _uow.SeasonRepository.Get(filter: s => s.Status == Consts.STATUS_ACTIVE,
                                                            orderBy: s => s.OrderByDescending(o => o.FromDate)))
                                                            .Select(s => _mapper.Map<AdminSeasonDataSet>(s));
                response.Data = seasons;
                response.Succeeded = true;
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

        public async Task<Response<AdminSeasonDataSet>> CreateSeason(CreateSeasonParam createSeasonParam)
        {
            Response<AdminSeasonDataSet> response = new Response<AdminSeasonDataSet>();
            if (createSeasonParam.Name == null || createSeasonParam.Name.Trim().Length < 1)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Tên mùa không hợp lệ!");
                return response;
            }
            Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
            if (DateTime.Compare(currentSeason.FromDate, createSeasonParam.FromDate) > 0)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Mùa mới trùng ngày với các mùa trước!");
                return response;
            }
            IEnumerable<Models.Season> seasons = await _uow.SeasonRepository.Get(filter: s => s.Name == createSeasonParam.Name
                                                                        && s.Status == Consts.STATUS_ACTIVE);
            if (seasons.Count() > 0)
            {
                response.Succeeded = false;
                if (response.Errors == null)
                {
                    response.Errors = new List<string>();
                }
                response.Errors.Add("Tên mùa đã tồn tại trong hệ thống!");
                return response;
            }
            using var tran = _uow.GetTransaction();
            try
            {
                Models.Season season = new Models.Season
                {
                    Name = createSeasonParam.Name,
                    FromDate = createSeasonParam.FromDate,
                    Status = Consts.STATUS_ACTIVE,
                };
                _uow.SeasonRepository.Insert(season);
                if ((await _uow.CommitAsync()) <= 0)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Thêm mùa mới bị lỗi!");
                    return response;
                }
                if (createSeasonParam.SeasonSourceId != null && createSeasonParam.SeasonSourceId > 0)
                {
                    IEnumerable<Models.MajorDetail> majorDetails = await _uow.MajorDetailRepository.Get(
                        filter: m => m.SeasonId == createSeasonParam.SeasonSourceId && m.Status == Consts.STATUS_ACTIVE,
                        includeProperties: "AdmissionCriterion,AdmissionCriterion.SubAdmissionCriteria,AdmissionCriterion.SubAdmissionCriteria.EntryMarks");

                    if (majorDetails.Any())
                    {
                        foreach (Models.MajorDetail oldMajorDetail in majorDetails)
                        {
                            //MAJORDETAIL
                            Models.MajorDetail newMajorDetail = new Models.MajorDetail
                            {
                                UniversityId = oldMajorDetail.UniversityId,
                                MajorId = oldMajorDetail.MajorId,
                                MajorCode = oldMajorDetail.MajorCode,
                                TrainingProgramId = oldMajorDetail.TrainingProgramId,
                                SeasonId = season.Id,
                                Status = Consts.STATUS_ACTIVE,
                                UpdatedDate = DateTime.UtcNow,
                            };
                            _uow.MajorDetailRepository.Insert(newMajorDetail);
                            if ((await _uow.CommitAsync()) <= 0)
                            {
                                response.Succeeded = false;
                                if (response.Errors == null)
                                {
                                    response.Errors = new List<string>();
                                }
                                response.Errors.Add("Thêm chi tiết ngành bị lỗi!");
                                return response;
                            }

                            //ADDMISSION
                            if (oldMajorDetail.AdmissionCriterion == null)
                            {
                                continue;
                            }
                            Models.AdmissionCriterion newAdmissionCriterion = new Models.AdmissionCriterion
                            {
                                MajorDetailId = newMajorDetail.Id,
                                Quantity = null,
                            };
                            _uow.AdmissionCriterionRepository.Insert(newAdmissionCriterion);
                            if ((await _uow.CommitAsync()) <= 0)
                            {
                                response.Succeeded = false;
                                if (response.Errors == null)
                                {
                                    response.Errors = new List<string>();
                                }
                                response.Errors.Add("Thêm chỉ tiêu tổng bị lỗi!");
                                return response;
                            }


                            IEnumerable<Models.SubAdmissionCriterion> oldSubAdmissions = (oldMajorDetail.AdmissionCriterion.SubAdmissionCriteria)
                                .Where(s => s.Status == Consts.STATUS_ACTIVE);
                            //SUBADMISSION
                            if (oldSubAdmissions == null || oldSubAdmissions.Count() < 1)
                            {
                                continue;
                            }
                            foreach (var oldSubAdmission in oldSubAdmissions)
                            {
                                Models.SubAdmissionCriterion newSubAdmissionCriterion = new Models.SubAdmissionCriterion
                                {
                                    AdmissionCriterionId = newAdmissionCriterion.MajorDetailId,
                                    Quantity = null,
                                    Gender = oldSubAdmission.Gender,
                                    ProvinceId = oldSubAdmission.ProvinceId,
                                    AdmissionMethodId = oldSubAdmission.AdmissionMethodId,
                                    Status = Consts.STATUS_ACTIVE
                                };
                                _uow.SubAdmissionCriterionRepository.Insert(newSubAdmissionCriterion);

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

                                //ENTRYMARKS

                                IEnumerable<Models.EntryMark> oldEntryMarks = (oldSubAdmission.EntryMarks).Where(e => e.Status == Consts.STATUS_ACTIVE);
                                if (oldEntryMarks == null || oldEntryMarks.Count() < 1)
                                {
                                    continue;
                                }
                                foreach (var oldEntryMark in oldEntryMarks)
                                {
                                    Models.EntryMark newEntryMark = new Models.EntryMark
                                    {
                                        MajorSubjectGroupId = oldEntryMark.MajorSubjectGroupId,
                                        Mark = null,
                                        SubAdmissionCriterionId = newSubAdmissionCriterion.Id,
                                        Status = Consts.STATUS_ACTIVE,
                                    };
                                    _uow.EntryMarkRepository.Insert(newEntryMark);
                                    if ((await _uow.CommitAsync()) <= 0)
                                    {
                                        response.Succeeded = false;
                                        if (response.Errors == null)
                                        {
                                            response.Errors = new List<string>();
                                        }
                                        response.Errors.Add("Thêm chỉ điểm bị lỗi!");
                                        return response;
                                    }
                                }
                            }

                        }
                    }
                }
                
                tran.Commit();
                response.Succeeded = true;
                response.Data = _mapper.Map<AdminSeasonDataSet>(season);
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

        public async Task<Response<bool>> UpdateSeason(UpdateSeasonParam updateSeasonParam)
        {
            Response<bool> response = new Response<bool>();
            using var tran = _uow.GetTransaction();
            try
            {
                Models.Season season = await _uow.SeasonRepository.GetById(updateSeasonParam.Id);
                if (season == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Mùa này không tồn tại!");
                    return response;
                }
                if (updateSeasonParam.Status == Consts.STATUS_INACTIVE)
                {
                    season.Status = Consts.STATUS_INACTIVE;
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Xóa mùa bị lỗi!");
                        return response;
                    }
                    IEnumerable<Models.MajorDetail> majorDetails = await _uow.MajorDetailRepository.Get(
                        filter: m => m.SeasonId == season.Id && m.Status == Consts.STATUS_ACTIVE,
                        includeProperties: "AdmissionCriterion");

                    if (majorDetails.Any())
                    {
                        foreach (Models.MajorDetail majorDetail in majorDetails)
                        {
                            //MAJORDETAIL
                            majorDetail.Status = Consts.STATUS_INACTIVE;
                            _uow.MajorDetailRepository.Update(majorDetail);
                            if ((await _uow.CommitAsync()) <= 0)
                            {
                                response.Succeeded = false;
                                if (response.Errors == null)
                                {
                                    response.Errors = new List<string>();
                                }
                                response.Errors.Add("Xóa ngành bị lỗi!");
                                return response;
                            }

                            //ADDMISSION
                            if (majorDetail.AdmissionCriterion == null)
                            {
                                continue;
                            }

                            IEnumerable<Models.SubAdmissionCriterion> subAdmissions = await _uow.SubAdmissionCriterionRepository.
                                   Get(filter: s => s.Id == majorDetail.Id && s.Status == Consts.STATUS_ACTIVE);

                            //SUBADMISSION
                            if (!subAdmissions.Any())
                            {
                                continue;
                            }

                            foreach (var subAdmission in subAdmissions)
                            {
                                subAdmission.Status = Consts.STATUS_INACTIVE;
                                _uow.SubAdmissionCriterionRepository.Update(subAdmission);

                                if ((await _uow.CommitAsync()) <= 0)
                                {
                                    response.Succeeded = false;
                                    if (response.Errors == null)
                                    {
                                        response.Errors = new List<string>();
                                    }
                                    response.Errors.Add("Xóa chỉ tiêu phụ bị lỗi!");
                                    return response;
                                }

                                //ENTRYMARKS

                                IEnumerable<Models.EntryMark> entryMarks = await _uow.EntryMarkRepository.Get(
                                    filter: e => e.Status == Consts.STATUS_ACTIVE && e.SubAdmissionCriterionId == subAdmission.Id);
                                if (!entryMarks.Any())
                                {
                                    continue;
                                }

                                foreach (var entryMark in entryMarks)
                                {
                                    entryMark.Status = Consts.STATUS_INACTIVE;
                                    _uow.EntryMarkRepository.Update(entryMark);
                                    if ((await _uow.CommitAsync()) <= 0)
                                    {
                                        response.Succeeded = false;
                                        if (response.Errors == null)
                                        {
                                            response.Errors = new List<string>();
                                        }
                                        response.Errors.Add("Xóa điểm chuẩn bị lỗi!");
                                        return response;
                                    }

                                    //Following
                                    IEnumerable<Models.FollowingDetail> followingDetails = await _uow.FollowingDetailRepository.Get(
                                    filter: f => f.Status == Consts.STATUS_ACTIVE && f.EntryMarkId == entryMark.Id);
                                    if (!entryMarks.Any())
                                    {
                                        continue;
                                    }

                                    foreach (var followingDetail in followingDetails)
                                    {
                                        followingDetail.Status = Consts.STATUS_INACTIVE;
                                        _uow.FollowingDetailRepository.Update(followingDetail);
                                        if ((await _uow.CommitAsync()) <= 0)
                                        {
                                            response.Succeeded = false;
                                            if (response.Errors == null)
                                            {
                                                response.Errors = new List<string>();
                                            }
                                            response.Errors.Add("Xóa theo dõi bị lỗi!");
                                            return response;
                                        }
                                    }
                                }
                            }
                        }
                    }
                } 
                else if (updateSeasonParam.Status == Consts.STATUS_ACTIVE)
                {
                    if (updateSeasonParam.Name == null || updateSeasonParam.Name.Trim().Length < 1)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Tên mùa không hợp lệ!");
                        return response;
                    }
                    Models.Season currentSeason = await _uow.SeasonRepository.GetCurrentSeason();
                    if (DateTime.Compare(currentSeason.FromDate, updateSeasonParam.FromDate) > 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Mùa mới trùng ngày với các mùa trước!");
                        return response;
                    }
                    IEnumerable<Models.Season> seasons = await _uow.SeasonRepository.Get(filter: s => s.Name == updateSeasonParam.Name
                                                                                && s.Status == Consts.STATUS_ACTIVE);
                    if (seasons.Count() > 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Tên mùa đã tồn tại trong hệ thống!");
                        return response;
                    }

                    season.Name = updateSeasonParam.Name;
                    season.FromDate = updateSeasonParam.FromDate;
                    if ((await _uow.CommitAsync()) <= 0)
                    {
                        response.Succeeded = false;
                        if (response.Errors == null)
                        {
                            response.Errors = new List<string>();
                        }
                        response.Errors.Add("Cập nhật mùa bị lỗi!");
                        return response;
                    }
                } else
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Trạng thái mùa cập nhật không hợp lệ!");
                    return response;
                }

                tran.Commit();
                response.Succeeded = true;
                response.Data = true;
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
