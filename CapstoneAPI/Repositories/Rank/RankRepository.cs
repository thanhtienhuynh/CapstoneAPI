using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories.Rank
{
    public class RankRepository : GenericRepository<Models.Rank>, IRankRepository
    {
        public RankRepository(CapstoneDBContext context) : base(context) { }

        public int CalculateRank(int transcriptId, double totalMark, IEnumerable<Models.Rank> ranks)
        {
            int rank = 0;
            if (ranks.Any())
            {
                if (transcriptId == Consts.RANK_TYPE_HB)
                {
                    int count = ranks.Where(r => r.TotalMark > totalMark
                                            || r.TranscriptTypeId == Consts.RANK_TYPE_HT
                                            || r.TranscriptTypeId == Consts.RANK_TYPE_THPTQG)
                                            .Count();
                    rank = count + 1;
                }
                else if (transcriptId == Consts.RANK_TYPE_HT)
                {
                    int count = ranks.Where(r => r.TranscriptTypeId != Consts.RANK_TYPE_HB
                                        && (r.TotalMark > totalMark
                                            || r.TranscriptTypeId == Consts.RANK_TYPE_THPTQG))
                                        .Count();
                    rank = count + 1;
                }
                else if (transcriptId == Consts.RANK_TYPE_THPTQG)
                {
                    int count = ranks.Where(r => r.TranscriptTypeId == Consts.RANK_TYPE_THPTQG
                                        && r.TotalMark > totalMark)
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
