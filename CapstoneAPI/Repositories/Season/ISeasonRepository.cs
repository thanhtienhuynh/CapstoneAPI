using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories.Season
{
    public interface ISeasonRepository : IGenericRepository<Models.Season>
    {
        Task<Models.Season> GetCurrentSeason();
        Task<Models.Season> GetPreviousSeason();
    }
}
