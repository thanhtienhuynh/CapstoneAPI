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
            DateTime currentDate = JWTUtils.GetCurrentTimeInVN();

            Models.Season season = (await Get(filter: s => s.FromDate <= currentDate && s.Status == Consts.STATUS_ACTIVE,
                                        orderBy: s => s.OrderByDescending(s => s.FromDate))).FirstOrDefault();
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
