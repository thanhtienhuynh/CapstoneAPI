using AutoMapper;
using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Season;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Season
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
                IEnumerable<AdminSeasonDataSet> seasons = (await _uow.SeasonRepository.Get(orderBy: s => s.OrderByDescending(o => o.FromDate)))
                                                            .Select(s => _mapper.Map<AdminSeasonDataSet>(s));
                response.Data = seasons;
                response.Succeeded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
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
                response.Errors.Add("Tên năm không hợp lệ!");
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
                response.Errors.Add("Năm đã tồn tại!");
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
                response.Errors.Add("Tên năm này đã tồn tại trong hệ thống!");
                return response;
            }
            using var tran = _uow.GetTransaction();
            try
            {
                Models.Season season = new Models.Season
                {
                    Name = createSeasonParam.Name,
                    FromDate = createSeasonParam.FromDate,
                    ToDate = createSeasonParam.ToDate,
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
                IEnumerable<Models.MajorDetail> majorDetails = await _uow.MajorDetailRepository.Get(filter: m =>
                m.SeasonId == createSeasonParam.SeasonSourceId && m.Status == Consts.STATUS_ACTIVE,
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
                        if (oldSubAdmissions == null|| oldSubAdmissions.Count() < 1)
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
    }
}
