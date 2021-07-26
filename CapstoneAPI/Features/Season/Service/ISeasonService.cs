using CapstoneAPI.DataSets;
using CapstoneAPI.Features.Season.DataSet;
using CapstoneAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Features.Season.Service
{
    public interface ISeasonService
    {
        Task<Response<IEnumerable<AdminSeasonDataSet>>> GetAllSeasons();
        Task<Response<AdminSeasonDataSet>> CreateSeason(CreateSeasonParam createSeasonParam);
        Task<Response<bool>> UpdateSeason(UpdateSeasonParam updateSeasonParam);
    }
}
