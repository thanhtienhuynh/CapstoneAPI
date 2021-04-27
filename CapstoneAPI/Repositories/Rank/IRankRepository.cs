using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories.Rank
{
    public interface IRankRepository : IGenericRepository<Models.Rank>
    {
        public int CalculateRank(int transcriptId, double totalMark, IEnumerable<Models.Rank> ranks);
    }
}
