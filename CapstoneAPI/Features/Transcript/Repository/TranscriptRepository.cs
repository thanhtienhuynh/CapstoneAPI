using CapstoneAPI.Features.Transcript.DataSet;
using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Transcript.Repository
{
    public class TranscriptRepository : GenericRepository<Models.Transcript>, ITranscriptRepository
    {
        public TranscriptRepository(CapstoneDBContext context) : base(context) { }

        public async Task<IEnumerable<UserTranscriptTypeDataSet>> GetUserTranscripts(int userId)
        {
            List<UserTranscriptTypeDataSet> result = new List<UserTranscriptTypeDataSet>();
            IEnumerable<Models.Transcript> transcripts = await Get(filter: t => t.UserId == userId
                                    && t.Status == Consts.STATUS_ACTIVE, includeProperties: "TranscriptType,Subject");
            if (transcripts == null || !transcripts.Any())
            {
                return result;
            }
            IEnumerable<IGrouping<Models.TranscriptType, Models.Transcript>> groupByTranscriptType = transcripts.GroupBy(g => g.TranscriptType).OrderByDescending(g => g.Key.Priority);
            foreach (var transcript in groupByTranscriptType)
            {
                UserTranscriptTypeDataSet userTranscriptDataSet = new UserTranscriptTypeDataSet
                {
                    Id = transcript.Key.Id,
                    Name = transcript.Key.Name,
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
            return result.OrderByDescending(t => t.Priority);
        }
    }
}
