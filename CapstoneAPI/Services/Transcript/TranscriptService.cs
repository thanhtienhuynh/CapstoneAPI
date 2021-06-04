using AutoMapper;
using CapstoneAPI.DataSets.Transcript;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Transcript
{
    public class TranscriptService :ITranscriptService
    {
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public TranscriptService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<UserTranscriptDataSet>>> GetMarkOfUser(string token)
        {
            Response<IEnumerable<UserTranscriptDataSet>> response = new Response<IEnumerable<UserTranscriptDataSet>>();
            List<UserTranscriptDataSet> result = new List<UserTranscriptDataSet>();
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
            IEnumerable<Models.Transcript> transcripts = await _uow.TranscriptRepository.Get(filter: t => t.UserId == userId,
                includeProperties: "TranscriptType,Subject");
            if (transcripts == null || !transcripts.Any())
            {
                response.Succeeded = true;
                return response;
            }
            IEnumerable<IGrouping<Models.TranscriptType, Models.Transcript>> groupByTranscriptType = transcripts.GroupBy(g => g.TranscriptType).OrderByDescending(g => g.Key.Priority);
            foreach (var transcript in groupByTranscriptType)
            {
                UserTranscriptDataSet userTranscriptDataSet = new UserTranscriptDataSet
                {
                    TranscriptTypeId = transcript.Key.Id,
                    TranscriptTypeName = transcript.Key.Name,
                    Priority = transcript.Key.Priority,
                };
                List<UserTranscriptDetailDataSet> userTranscriptDetailDataSets = new List<UserTranscriptDetailDataSet>();
                foreach (var transcriptDetail in transcript)
                {
                    UserTranscriptDetailDataSet userTranscriptDetailDataSet = new UserTranscriptDetailDataSet
                    {
                        TransriptId = transcriptDetail.Id,
                        Mark = transcriptDetail.Mark,
                        DateRecord = transcriptDetail.DateRecord,
                        SubjectId = transcriptDetail.Subject.Id,
                        SubjectName = transcriptDetail.Subject.Name,
                    };
                    userTranscriptDetailDataSets.Add(userTranscriptDetailDataSet);
                }
                userTranscriptDataSet.TranscriptDetails = userTranscriptDetailDataSets;
                result.Add(userTranscriptDataSet);
            }
            response.Succeeded = true;
            response.Data = result;
            return response;
        }
    }
}
