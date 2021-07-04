using AutoMapper;
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
    }
}
