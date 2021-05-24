using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Repositories.Season
{
    public class SeasonRepository : GenericRepository<Models.Season>, ISeasonRepository
    {
        public SeasonRepository(CapstoneDBContext context) : base(context) { }

        public async Task<Models.Season> GetCurrentSeason()
        {
            DateTime currentDate = DateTime.UtcNow;
            Models.Season season = await GetFirst(s => s.Status == Consts.STATUS_ACTIVE && s.FromDate <= currentDate && s.ToDate >= currentDate);
            return season;
        }

        public async Task<Models.Season> GetPreviousSeason()
        {
            Models.Season currentSeason = await GetCurrentSeason();
            Models.Season previousSeason = (await Get(filter: s => s.FromDate < currentSeason.FromDate)).OrderByDescending(s => s.FromDate).FirstOrDefault();
            return previousSeason;
        }
    }
}
