using AutoMapper;
using CapstoneAPI.Features.SubjectGroup.DataSet;
using CapstoneAPI.Features.Transcript.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
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

        public TranscriptService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<UserTranscriptTypeDataSet>>> GetMarkOfUser(string token)
        {
            Response<IEnumerable<UserTranscriptTypeDataSet>> response = new Response<IEnumerable<UserTranscriptTypeDataSet>>();
            try
            {
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
                if (userIdString == null && userIdString.Length <= 0)
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
                IEnumerable<UserTranscriptTypeDataSet> result = await _uow.TranscriptRepository.GetUserTranscripts(userId);
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
                if (userIdString == null && userIdString.Length <= 0)
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
                Models.User user = await _uow.UserRepository.GetById(userId);
                if (user == null)
                {
                    response.Succeeded = false;
                    if (response.Errors == null)
                    {
                        response.Errors = new List<string>();
                    }
                    response.Errors.Add("Tài khoản của bạn không tồn tại!");
                    return response;
                }
                List<Models.Transcript> newTranscripts = new List<Models.Transcript>();
                foreach (MarkParam markParam in subjectGroupParam.Marks)
                {
                    int transcriptTypeId = subjectGroupParam.TranscriptTypeId;
                    IEnumerable<Models.Transcript> transcripts = await _uow.TranscriptRepository
                                            .Get(filter: t => t.SubjectId == markParam.SubjectId
                                                        && t.UserId == userId
                                                        && t.TranscriptTypeId == transcriptTypeId && t.Status == Consts.STATUS_ACTIVE);
                    newTranscripts.Add(new Models.Transcript()
                    {
                        Mark = markParam.Mark,
                        SubjectId = markParam.SubjectId,
                        UserId = userId,
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

                if (user.Gender != subjectGroupParam.Gender || user.ProvinceId != subjectGroupParam.ProvinceId)
                {
                    user.Gender = subjectGroupParam.Gender;
                    user.ProvinceId = subjectGroupParam.ProvinceId;
                    _uow.UserRepository.Update(user);
                }

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
