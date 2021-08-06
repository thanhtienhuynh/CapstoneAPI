using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Rank.Service
{
    public interface IRankService
    {
        Task<Response<bool>> UpdateRank();
    }
}
