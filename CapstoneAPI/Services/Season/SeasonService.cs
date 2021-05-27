using AutoMapper;
using CapstoneAPI.DataSets;
using CapstoneAPI.Helpers;
using CapstoneAPI.Repositories;
using CapstoneAPI.Wrappers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Services.Season
{
    public class SeasonService : ISeasonService
    {
        private readonly IUnitOfWork _uow;
        public SeasonService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
        }

        public async Task<Response<IEnumerable<Models.Season>>> GetAllSeasons()
        {
            Response<IEnumerable<Models.Season>> response = new Response<IEnumerable<Models.Season>>();
            IEnumerable<Models.Season> seasons = (await _uow.SeasonRepository.Get(filter: s => s.Status == Consts.STATUS_ACTIVE, orderBy: s => s.OrderByDescending(o => o.FromDate)));

            response.Data = seasons;
            response.Message = "Thành công!";
            response.Succeeded = true;

            return response;
        }
    }
}
