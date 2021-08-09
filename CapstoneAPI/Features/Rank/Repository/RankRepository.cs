using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Rank.Repository
{
    public class RankRepository : GenericRepository<Models.Rank>, IRankRepository
    {
        public RankRepository(CapstoneDBContext context) : base(context) { }

        public int CalculateRank(int transcriptId, double totalMark, IEnumerable<Models.Rank> ranks, double entryMark)
        {
            int rank = 0;
            if (ranks.Any())
            {
                if (totalMark >= entryMark)
                {
                    if (transcriptId == TranscriptTypes.HocBa)
                    {
                        int count = ranks.Where(r => r.TotalMark >= entryMark &&
                            (r.TotalMark > totalMark || r.TranscriptTypeId == TranscriptTypes.ThiThu
                                                || r.TranscriptTypeId == TranscriptTypes.THPTQG))
                                                .Count();
                        rank = count + 1;
                    }
                    else if (transcriptId == TranscriptTypes.ThiThu)
                    {
                        int count = ranks.Where(r => r.TranscriptTypeId != TranscriptTypes.HocBa
                                            && r.TotalMark >= entryMark
                                            && (r.TotalMark > totalMark
                                                || r.TranscriptTypeId == TranscriptTypes.THPTQG))
                                            .Count();
                        rank = count + 1;
                    }
                    else if (transcriptId == TranscriptTypes.THPTQG)
                    {
                        int count = ranks.Where(r => r.TranscriptTypeId == TranscriptTypes.THPTQG
                                            && r.TotalMark > totalMark && r.TotalMark >= entryMark)
                                            .Count();
                        rank = count + 1;
                    }
                }
                else
                {
                    int count = ranks.Where(r => r.TotalMark >= entryMark)
                                            .Count();
                    rank = count + 1;
                }
                
            }
            else
            {
                rank = 1;
            }

            return rank;
        }

    }
}
