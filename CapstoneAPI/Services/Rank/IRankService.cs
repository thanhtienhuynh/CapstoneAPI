using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Rank
{
    public interface IRankService
    {
        Task<bool> UpdateRank();
    }
}
