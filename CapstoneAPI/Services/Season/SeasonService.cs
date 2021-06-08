using AutoMapper;
using CapstoneAPI.DataSets;
using CapstoneAPI.DataSets.Season;
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
        private IMapper _mapper;
        public SeasonService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<AdminSeasonDataSet>>> GetAllSeasons()
        {
            Response<IEnumerable<AdminSeasonDataSet>> response = new Response<IEnumerable<AdminSeasonDataSet>>();
            IEnumerable<AdminSeasonDataSet> seasons =  (await _uow.SeasonRepository.Get(filter: s=> s.Status == Consts.STATUS_ACTIVE,orderBy: s => s.OrderByDescending(o => o.FromDate)))
                                                            .Select(s => _mapper.Map<AdminSeasonDataSet>(s));
            response.Data = seasons;
            response.Message = "Thành công!";
            response.Succeeded = true;

            return response;
        }
    }
}
