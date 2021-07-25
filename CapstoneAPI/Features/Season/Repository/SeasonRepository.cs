using CapstoneAPI.Helpers;
using CapstoneAPI.Models;
using CapstoneAPI.Repositories;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneAPI.Features.Season.Repository
{
    public class SeasonRepository : GenericRepository<Models.Season>, ISeasonRepository
    {
        public SeasonRepository(CapstoneDBContext context) : base(context) {
        }

        public async Task<Models.Season> GetCurrentSeason()
        {
            DateTime currentDate = DateTime.UtcNow;
            SeasonView seasonView = await _context.SeasonViews.Where(s => s.Status == Consts.STATUS_ACTIVE &&
                    ((currentDate >= s.FromDate && currentDate <= s.ToDate)
                    || (currentDate >= s.FromDate && s.ToDate == null))).FirstOrDefaultAsync();
            if (seasonView == null)
            {
                return null;
            }
            Models.Season season = new Models.Season()
            {
                Id = seasonView.Id,
                FromDate = seasonView.FromDate,
                Name = seasonView.Name,
                Status = seasonView.Status
            };
            return season;
        }

        public async Task<Models.Season> GetPreviousSeason()
        {
            Models.Season currentSeason = await GetCurrentSeason();
            Models.Season previousSeason = null;
            if (currentSeason != null)
            {
                previousSeason = (await Get(filter: s => s.FromDate < currentSeason.FromDate
                                    && s.Status == Consts.STATUS_ACTIVE))
                                    .OrderByDescending(s => s.FromDate).FirstOrDefault();
            }
            return previousSeason;
        }
    }
}
