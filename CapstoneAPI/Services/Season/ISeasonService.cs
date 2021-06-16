using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Season;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Season
{
    public interface ISeasonService
    {
        Task<Response<IEnumerable<AdminSeasonDataSet>>> GetAllSeasons();
        Task<Response<AdminSeasonDataSet>> CreateSeason(CreateSeasonParam createSeasonParam);
    }
}
