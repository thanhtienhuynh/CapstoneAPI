using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Rank.Service
{
    public interface IRankService
    {
        Task<bool> UpdateRank();
    }
}
